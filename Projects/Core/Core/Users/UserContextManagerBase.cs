﻿using OnUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Transactions;

namespace OnXap.Users
{
    using Core;
    using Core.Db;
    using Journaling;
    using ExecutionPermissionsResult = ExecutionResult<UserPermissions>;

    /// <summary>
    /// Менеджер, управляющий контекстами пользователей (см. <see cref="IUserContext"/>).
    /// Каждый поток приложения имеет ассоциированный контекст пользователя, от имени которого могут выполняться запросы и выполняться действия. 
    /// Более подробно см. <see cref="GetCurrentUserContext"/> / <see cref="SetCurrentUserContext(IUserContext)"/> / <see cref="ClearCurrentUserContext"/>.
    /// </summary>
    public class UserContextManagerBase : CoreComponentBase, IComponentSingleton
    {
        /// <summary>
        /// Код ошибки авторизации пользователя.
        /// </summary>
        public const int EventCodeAuthError = 1000001;

        /// <summary>
        /// Код успешной авторизации пользователя.
        /// </summary>
        public const int EventCodeAuthSuccess = 1000002;

        public const string RoleUserName = "RoleUser";
        public const string RoleGuestName = "RoleGuest";

        private static IUserContext _systemUserContext;
        private ThreadLocal<IUserContext> _currentUserContext = new ThreadLocal<IUserContext>();
        private int? _journalAuthId = null;

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStarted()
        {
            this.RegisterJournal("Менеджер контекстов пользователей");

            var journalingManager = AppCore.Get<JournalingManager>();
            var authJournalResult = journalingManager.RegisterJournal(1, "Журнал авторизации пользователей", "UsersAuthJournal");
            if (authJournalResult.IsSuccess)
            {
                _journalAuthId = authJournalResult.Result.IdJournal;
            }
            else
            {
                this.RegisterEvent(EventType.CriticalError, "Ошибка регистрации журнала авторизации пользователей.", authJournalResult.Message);
            }

            User systemUser = null;
            try
            {
                User systemUser2 = null;

                using (var db = new CoreContext())
                {
                    var systemUserKey = "System";
                    var user = db.Users.Where(x => x.UniqueKey == systemUserKey).FirstOrDefault();
                    if (user == null)
                    {
                        user = new User()
                        {
                            DateChange = DateTime.Now.Timestamp(),
                            IdUserChange = 1,
                            Superuser = 1,
                            name = "System",
                            password = DateTime.Now.Ticks.ToString().GenerateGuid().ToString(),
                            UniqueKey = systemUserKey
                        };
                        db.Users.Add(user);
                        db.SaveChanges();
                    }
                    else
                    {
                        user.Superuser = 1;
                        user.name = "System";
                        db.SaveChanges();
                    }

                    systemUser2 = user;
                }

                systemUser = systemUser2;
            }
            catch (Exception ex)
            {
                this.RegisterEvent(EventType.CriticalError, "Не удалось получить системного пользователя", null, ex);
                throw new HandledException("Ошибка запуска менеджера контекстов пользователей. Не удалось получить системного пользователя.", ex);
            }

            var systemUserContext = new UserContext(systemUser, true);
            ((IComponentStartable)systemUserContext).Start(AppCore);
            _systemUserContext = systemUserContext;

            AppCore.Get<Modules.Subscriptions.SubscriptionsManager>().UpdateSubscribers(
                AppCore.Get<Modules.CoreModule.CoreModule>()._subscriptionEventCritical, 
                Modules.Subscriptions.ChangeType.Append, 
                new int[] { _systemUserContext.IdUser });

            // В момент запуска для запускающего потока устанавливается системный пользователь для выполнения инициализирующих действий с максимальными правами доступа.
            _currentUserContext.Value = _systemUserContext;
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
        }
        #endregion

        #region Методы
        /// <summary>
        /// Возвращает контекст системного пользователя.
        /// </summary>
        public virtual IUserContext GetSystemUserContext()
        {
            return _systemUserContext;
        }

        /// <summary>
        /// Возвращает контекст пользователя, ассоциированный с текущим потоком выполнения. 
        /// По-умолчанию возвращается контекст системного пользователя, если не задан иной контекст путем вызова <see cref="SetCurrentUserContext(IUserContext)"/>.
        /// </summary>
        public virtual IUserContext GetCurrentUserContext()
        {
            if (!_currentUserContext.IsValueCreated) ClearCurrentUserContext();
            return _currentUserContext.Value;
        }

        /// <summary>
        /// Устанавливает текущий контекст пользователя. Для замены текущего контекста достаточно заново вызвать этот метод, вызывать <see cref="ClearCurrentUserContext"/> для сброса контекста необязательно.
        /// </summary>
        /// <param name="context">Новый контекст пользователя. Не должен быть равен null.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="context"/> равен null.</exception>
        public virtual void SetCurrentUserContext(IUserContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _currentUserContext.Value = context;
        }

        /// <summary>
        /// Устанавливает контекст гостя в качестве текущего контекста, сбрасывая любой предыдущий установленный контекст.
        /// </summary>
        public virtual void ClearCurrentUserContext()
        {
            _currentUserContext.Value = CreateGuestUserContext();
        }

        #region Создать контекст
        /// <summary>
        /// Возвращает контекст гостя.
        /// </summary>
        public virtual IUserContext CreateGuestUserContext()
        {
            return new UserContext(new User() { IdUser = 0, Superuser = 0 }, false);
        }

        /// <summary>
        /// Возвращает контекст пользователя с идентификатором <paramref name="idUser"/>.
        /// </summary>
        /// <param name="idUser">Идентификатор пользователя.</param>
        /// <param name="userContext">Содержит контекст в случае успеха.</param>
        /// <returns>Возвращает результат создания контекста.</returns>
        [ApiIrreversible]
        public UserContextCreateResult CreateUserContext(int idUser, out IUserContext userContext)
        {
            userContext = null;
            if (idUser == GetSystemUserContext().IdUser) return UserContextCreateResult.NotFound;

            using (var db = new CoreContext())
            using (var scope = db.CreateScope(TransactionScopeOption.RequiresNew))
            {
                try
                {
                    var res = db.Users.Where(x => x.IdUser == idUser).FirstOrDefault();
                    if (res == null) return UserContextCreateResult.NotFound;

                    var context = new UserContext(res, true);
                    ((IComponentStartable)context).Start(AppCore);

                    var permissionsResult = GetPermissions(context.IdUser);
                    if (!permissionsResult.IsSuccess)
                    {
                        return UserContextCreateResult.ErrorReadingPermissions;
                    }
                    context.ApplyPermissions(permissionsResult.Result);
                    userContext = context;
                    return UserContextCreateResult.Success;
                }
                catch (Exception ex)
                {
                    this.RegisterEvent(EventType.CriticalError, "Неизвестная ошибка во время создания контекста пользователя.", $"IdUser={idUser}'.", null, ex);
                    userContext = null;
                    return UserContextCreateResult.ErrorUnknown;
                }
                finally
                {
                    scope.Complete();
                }
            }
        }
        #endregion

        public void DestroyUserContext(IUserContext context)
        {

        }

        /// <summary>
        /// Возвращает идентификатор журнала авторизации пользователей.
        /// </summary>
        /// <returns></returns>
        public int? GetAuthJournalId()
        {
            return _journalAuthId;
        }
        #endregion

        #region Разрешения
        /// <summary>
        /// Возвращает список разрешений для пользователя <paramref name="idUser"/>.
        /// </summary>
        /// <returns>Возвращает объект <see cref="ExecutionPermissionsResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        [ApiIrreversible]
        public ExecutionPermissionsResult GetPermissions(int idUser)
        {
            try
            {
                using (var db = new CoreContext())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress))
                {
                    var idRoleUser = AppCore.AppConfig.RoleUser;
                    var idRoleGuest = AppCore.AppConfig.RoleGuest;

                    var perms2 = (from p in db.RolePermission
                                  join ru in db.RoleUser on p.IdRole equals ru.IdRole into gj
                                  from subru in gj.DefaultIfEmpty()
                                  where (subru.IdUser == idUser) || (idUser > 0 && p.IdRole == idRoleUser) || (idUser == 0 && p.IdRole == idRoleGuest)
                                  select new { p.IdModule, p.Permission });

                    var perms = new Dictionary<Guid, List<Guid>>();
                    foreach (var res in perms2)
                    {
                        if (!string.IsNullOrEmpty(res.Permission))
                        {
                            var guidModule = GuidIdentifierGenerator.GenerateGuid(GuidType.Module, res.IdModule);
                            if (!perms.ContainsKey(guidModule)) perms.Add(guidModule, new List<Guid>());

                            var guidPermission = res.Permission.GenerateGuid();
                            if (!perms[guidModule].Contains(guidPermission)) perms[guidModule].Add(guidPermission);

                            // Временное двойное распознавание разрешения через строку и потенциальный гуид.
                            if (Guid.TryParse(res.Permission, out var guidPermissionTemp) && !perms[guidModule].Contains(guidPermissionTemp)) perms[guidModule].Add(guidPermissionTemp);
                        }
                    }

                    return new ExecutionPermissionsResult(true, null, new UserPermissions(perms));
                }
            }
            catch (Exception ex)
            {
                this.RegisterEvent(EventType.Error, "Ошибка при получении разрешений для пользователя.", $"IdUser={idUser}.", null, ex);
                return new ExecutionPermissionsResult(false, "Ошибка при получении разрешений для пользователя.");
            }
        }

        /// <summary>
        /// Пытается получить текущие разрешения для пользователя, ассоциированного с контекстом <paramref name="context"/>, и задать их контексту.
        /// </summary>
        /// <returns>Возвращает true, если удалось получить разрешения и установить их для переданного контекста.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="context"/> равен null.</exception>
        [ApiIrreversible]
        public virtual ExecutionResult TryRestorePermissions(IUserContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context is UserContext userContext)
            {
                var permissionsResult = GetPermissions(context.IdUser);
                if (permissionsResult.IsSuccess)
                {
                    userContext.ApplyPermissions(permissionsResult.Result);
                    return new ExecutionResult(true);
                }
                else return new ExecutionResult(false, permissionsResult.Message);
            }
            else return new ExecutionResult(false, "Неподдерживаемый тип контекста.");
        }
        #endregion
    }
}
