using OnUtils.Data;
using OnXap.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Transactions;

namespace OnXap.Users
{
    using Core;
    using CoreDB = Core.Db;
    using Journaling;
    using Core.Items;

    /// <summary>
    /// Менеджер, управляющий контекстами пользователей (см. <see cref="IUserContext"/>).
    /// Каждый поток приложения имеет ассоциированный контекст пользователя, от имени которого могут выполняться запросы и выполняться действия. 
    /// </summary>
    /// <seealso cref="UserContextManagerBase.GetCurrentUserContext"/>
    /// <seealso cref="UserContextManagerBase.SetCurrentUserContext(IUserContext)"/>
    /// <seealso cref="UserContextManagerBase.ClearCurrentUserContext"/>
    public class UserContextManager : UserContextManagerBase
    {
        #region Методы
        /// <summary>
        /// Возвращает контекст пользователя с идентификатором <paramref name="idUser"/>.
        /// </summary>
        /// <param name="idUser">Идентификатор пользователя.</param>
        /// <param name="userContext">Содержит контекст в случае успеха.</param>
        /// <returns>Возвращает результат создания контекста.</returns>
        [ApiIrreversible]
        public eAuthResult CreateUserContext(int idUser, out IUserContext userContext)
        {
            return CreateUserContext(idUser, null, null, out userContext, out var resultReason);
        }

        /// <summary>
        /// Возвращает контекст пользователя с идентификатором <paramref name="idUser"/>.
        /// </summary>
        /// <param name="idUser">Идентификатор пользователя.</param>
        /// <param name="userContext">Содержит контекст в случае успеха.</param>
        /// <param name="resultReason">Содержит текстовое пояснение к ответу функции.</param>
        /// <returns>Возвращает результат создания контекста.</returns>
        [ApiIrreversible]
        public eAuthResult CreateUserContext(int idUser, out IUserContext userContext, out string resultReason)
        {
            return CreateUserContext(idUser, null, null, out userContext, out resultReason);
        }

        /// <summary>
        /// Возвращает контекст пользователя с указанными реквизитами <paramref name="login"/>/<paramref name="password"/>. 
        /// </summary>
        /// <param name="login">Логин для авторизации. В качестве логина может выступать Email-адрес или номер телефона (в зависимости от настроек системы).</param>
        /// <param name="password">Пароль для авторизации. Должен передаваться в незашифрованном виде.</param>
        /// <param name="userContext">Содержит контекст в случае успеха.</param>
        /// <returns>Возвращает результат создания контекста.</returns>
        [ApiIrreversible]
        public eAuthResult CreateUserContext(string login, string password, out IUserContext userContext)
        {
            return CreateUserContext(0, login, password, out userContext, out var resultReason);
        }

        /// <summary>
        /// Возвращает контекст пользователя с указанными реквизитами <paramref name="login"/>/<paramref name="password"/>. 
        /// </summary>
        /// <param name="login">Логин для авторизации. В качестве логина может выступать Email-адрес или номер телефона (в зависимости от настроек системы).</param>
        /// <param name="password">Пароль для авторизации. Должен передаваться в незашифрованном виде.</param>
        /// <param name="userContext">Содержит контекст в случае успеха.</param>
        /// <param name="resultReason">Содержит текстовое пояснение к ответу функции.</param>
        /// <returns>Возвращает результат создания контекста.</returns>
        [ApiIrreversible]
        public eAuthResult CreateUserContext(string login, string password, out IUserContext userContext, out string resultReason)
        {
            return CreateUserContext(0, login, password, out userContext, out resultReason);
        }

        private eAuthResult CreateUserContext(int IdUser, string user, string password, out IUserContext userContext, out string resultReason)
        {
            var authorizationAttemptsExceeded = false;
            var id = 0;
            userContext = null;
            resultReason = null;
            Modules.Auth.ModuleConfiguration authConfig = null;

            using (var db = new CoreDB.CoreContext())
            using (var scope = db.CreateScope(TransactionScopeOption.RequiresNew))
            {
                var returnNewFailedResultWithAuthAttempt = new Func<string, string>(message =>
                {
                    RegisterLogHistoryEvent(id, EventType.Error, EventCodeAuthError, "Ошибка авторизации", message);

                    if (id > 0)
                    {
                        db.DataContext.ExecuteQuery(
                            $"UPDATE users SET AuthorizationAttempts = (AuthorizationAttempts + 1){(authorizationAttemptsExceeded ? ", BlockedUntil=@BlockedUntil, BlockedReason=@BlockedReason" : "")} WHERE id=@IdUser",
                            new
                            {
                                IdUser = id,
                                BlockedUntil = DateTime.Now.Timestamp() + authConfig.AuthorizationAttemptsBlock,
                                BlockedReason = authConfig.AuthorizationAttemptsBlockMessage,
                            }
                        );
                    }

                    return message + (authorizationAttemptsExceeded ? " " + authConfig.AuthorizationAttemptsMessage : "");
                });

                try
                {
                    authConfig = AppCore.Get<OnXap.Modules.Auth.ModuleAuth>()?.GetConfiguration<OnXap.Modules.Auth.ModuleConfiguration>();

                    var checkLoginResult = CheckLogin(IdUser, user, password, db, out var res);
                    if (!checkLoginResult.IsSuccess)
                    {
                        resultReason = returnNewFailedResultWithAuthAttempt(checkLoginResult.Message);
                        return checkLoginResult.AuthResult;
                    }

                    id = res.IdUser;
                    var attempts = authConfig.AuthorizationAttempts;
                    authorizationAttemptsExceeded = attempts > 0 && (res.AuthorizationAttempts + 1) >= attempts;

                    AppCore.Get<UsersManager>().getUsers(new Dictionary<int, CoreDB.User>() { { id, res } });

                    var context = new UserContext(res, true);
                    ((IComponentStartable)context).Start(AppCore);

                    var permissionsResult = AppCore.GetUserContextManager().GetPermissions(context.IdUser);
                    if (!permissionsResult.IsSuccess)
                    {
                        resultReason = returnNewFailedResultWithAuthAttempt(permissionsResult.Message);
                        return eAuthResult.UnknownError;
                    }
                    context.ApplyPermissions(permissionsResult.Result);

                    RegisterLogHistoryEvent(id, EventType.Info, EventCodeAuthSuccess, "Успешный вход");

                    res.AuthorizationAttempts = 0;
                    db.SaveChanges();

                    userContext = context;

                    var checkStateResult = CheckUserState(res, res.Comment);
                    if (checkStateResult.IsSuccess)
                    {
                        return eAuthResult.Success;
                    }
                    else
                    {
                        resultReason = returnNewFailedResultWithAuthAttempt(checkStateResult.Message);
                        return checkStateResult.AuthResult;
                    }
                }
                catch (Exception ex)
                {
                    this.RegisterEvent(EventType.CriticalError, "Неизвестная ошибка во время получения контекста пользователя.", $"IdUser={IdUser}, Login='{user}'.", null, ex);
                    userContext = null;
                    resultReason = "Неизвестная ошибка во время получения контекста пользователя.";
                    return eAuthResult.UnknownError;
                }
                finally
                {
                    scope.Commit();
                }
            }
        }

        private ExecutionAuthResult CheckLogin(int idUser, string login, string password, CoreDB.CoreContext db, out CoreDB.User outData)
        {
            outData = null;
            if (idUser == (int.MaxValue - 1))
            {
                return new ExecutionAuthResult(eAuthResult.NothingFound, $"Пользователь '{login}' не найден в базе данных.");
            }

            try
            {
                if (idUser <= 0 && string.IsNullOrEmpty(login)) return new ExecutionAuthResult(eAuthResult.WrongAuthData, "Не указаны реквизиты для авторизации!");

                List<CoreDB.User> query = null;
                bool directAuthorize = false;

                // Если в $user передан id и $password не передан вообще.
                if (idUser > 0)
                {
                    query = db.Users.Where(x => x.IdUser == idUser).ToList();
                    directAuthorize = true;
                }

                // Если Email
                if (query == null && login.isEmail())
                {
                    switch (AppCore.WebConfig.userAuthorizeAllowed)
                    {
                        case eUserAuthorizeAllowed.Nothing:
                            return new ExecutionAuthResult(eAuthResult.AuthDisabled, "Авторизация запрещена.");

                        case eUserAuthorizeAllowed.OnlyPhone:
                            return new ExecutionAuthResult(eAuthResult.AuthMethodNotAllowed, "Авторизация возможна только по номеру телефона.");

                        case eUserAuthorizeAllowed.EmailAndPhone:
                            query = (from p in db.Users where string.Compare(p.email, login, true) == 0 select p).ToList();
                            break;

                        case eUserAuthorizeAllowed.OnlyEmail:
                            query = (from p in db.Users where string.Compare(p.email, login, true) == 0 select p).ToList();
                            break;
                    }
                }

                // Если номер телефона
                if (query == null)
                {
                    var phone = PhoneBuilder.ParseString(login);
                    if (phone.IsCorrect)
                    {
                        switch (AppCore.WebConfig.userAuthorizeAllowed)
                        {
                            case eUserAuthorizeAllowed.Nothing:
                                return new ExecutionAuthResult(eAuthResult.AuthDisabled, "Авторизация запрещена.");

                            case eUserAuthorizeAllowed.OnlyEmail:
                                return new ExecutionAuthResult(eAuthResult.AuthMethodNotAllowed, "Авторизация возможна только через электронную почту.");

                            case eUserAuthorizeAllowed.EmailAndPhone:
                                query = (from p in db.Users where string.Compare(p.phone, phone.ParsedPhoneNumber, true) == 0 select p).ToList();
                                break;

                            case eUserAuthorizeAllowed.OnlyPhone:
                                query = (from p in db.Users where string.Compare(p.phone, phone.ParsedPhoneNumber, true) == 0 select p).ToList();
                                break;
                        }
                    }
                    else
                    {
                        if (!login.isEmail())
                        {
                            return new ExecutionAuthResult(eAuthResult.WrongAuthData, "Переданные данные не являются ни номером телефона, ни адресом электронной почты.");
                        }
                    }
                }

                if (query == null)
                {
                    return new ExecutionAuthResult(eAuthResult.UnknownError, "Что-то пошло не так во время авторизации.");
                }

                if (query.Count == 1)
                {
                    var res = query.First();

                    if (directAuthorize || res.password == UsersExtensions.hashPassword(password))
                    {
                        outData = res;
                        return new ExecutionAuthResult(eAuthResult.Success);
                    }
                    else
                    {
                        return new ExecutionAuthResult(eAuthResult.WrongPassword, "Неверный пароль.");
                    }
                }
                else if (query.Count > 1)
                {
                    AppCore.Get<MessagingManager>().GetCriticalMessagesReceivers().ForEach(x => x.SendToAdmin("Одинаковые реквизиты входа!", "Найдено несколько пользователей с логином '" + login + "'"));
                    return new ExecutionAuthResult(eAuthResult.MultipleFound, "Найдено несколько пользователей с логином '" + login + "'. Обратитесь к администратору для решения проблемы.");
                }
                else
                {
                    return new ExecutionAuthResult(eAuthResult.NothingFound, $"Пользователь '{login}' не найден в базе данных.");
                }
            }
            catch (Exception ex)
            {
                this.RegisterEvent(EventType.Error, "Ошибка во время поиска и проверки пользователя", $"IdUser={idUser}, Login='{login}'.", null, ex);
                return new ExecutionAuthResult(eAuthResult.UnknownError, "Неизвестная ошибка во время проверки авторизации.");
            }
        }

        private ExecutionAuthResult CheckUserState(CoreDB.User data, string comment = null)
        {
            if (data.State == CoreDB.UserState.Active)
            {
                if (data.BlockedUntil > DateTime.Now.Timestamp())
                {
                    return new ExecutionAuthResult(eAuthResult.BlockedUntil, "Учетная запись заблокирована до " + (new DateTime()).FromUnixtime(data.BlockedUntil).ToString("yyyy-mm-dd HH:MM") +
                        (!string.IsNullOrEmpty(data.BlockedReason) ? " по причине: " + data.BlockedReason : "."));
                }

                return new ExecutionAuthResult(eAuthResult.Success);
            }
            else if (data.State == CoreDB.UserState.RegisterNeedConfirmation)
            {
                return new ExecutionAuthResult(eAuthResult.RegisterNeedConfirmation, "Необходимо подтвердить регистрацию путем перехода по ссылке из письма, отправленного на указанный при регистрации Email-адрес.");
            }
            else if (data.State == CoreDB.UserState.RegisterWaitForModerate)
            {
                return new ExecutionAuthResult(eAuthResult.RegisterWaitForModerate, "Заявка на регистрацию еще не проверена администратором.");
            }
            else if (data.State == CoreDB.UserState.RegisterDecline)
            {
                var msg = "Заявка на регистрацию отклонена администратором.";
                return new ExecutionAuthResult(eAuthResult.RegisterDecline, !string.IsNullOrEmpty(comment) ? $"{msg}\r\n\r\nПричина: {comment}" : msg);
            }
            else if (data.State == CoreDB.UserState.Disabled)
            {
                var msg = "Учетная запись отключена.";
                return new ExecutionAuthResult(eAuthResult.Disabled, !string.IsNullOrEmpty(comment) ? $"{msg}\r\n\r\nПричина: {comment}" : msg);
            }
            else
            {
                return new ExecutionAuthResult(eAuthResult.UnknownError, "Ошибка при авторизации");
            }
        }
        #endregion

        private void RegisterLogHistoryEvent(int idUser, EventType eventType, int eventCode, string eventInfo, string Comment = null)
        {
            try
            {
                var idJournal = GetAuthJournalId();
                if (idJournal.HasValue)
                {
                    AppCore.Get<JournalingManager>().RegisterEventForItem(
                        idJournal.Value,
                        new ItemKey(ItemTypeFactory.GetItemType<CoreDB.User>().IdItemType, idUser, ""),
                        eventType,
                        eventCode,
                        eventInfo,
                        Comment);
                }
            }
            catch (Exception ex)
            {
                var idJournal = GetAuthJournalId();
                if (idJournal.HasValue)
                {
                    AppCore.Get<JournalingManager>().RegisterEvent(idJournal.Value, EventType.CriticalError, 0, "Ошибка регистрации события в журнал", null, null, ex);
                }
            }
        }
    }
}
