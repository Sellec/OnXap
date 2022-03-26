using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web.Mvc;

namespace OnXap.Modules.Customer
{
    using Core.Db;
    using Core.Modules;
    using Journaling;
    using MessagingEmail;

    public class ModuleControllerAdminCustomer : ModuleControllerAdmin<ModuleCustomer>
    {
        private const int JournalEventBase = 20000;

        /// <summary>
        /// Событие сохранения данных пользователя.
        /// </summary>
        public const int JournalEventUserEdit = JournalEventBase + 1;

        /// <summary>
        /// Событие изменения статуса заявки пользователя - принято.
        /// </summary>
        public const int JournalEventUserStateAccept = JournalEventBase + 2;

        /// <summary>
        /// Событие изменения статуса заявки пользователя - отклонено.
        /// </summary>
        public const int JournalEventUserStateDecline = JournalEventBase + 3;

        /// <summary>
        /// Событие изменения статуса пользователя - заблокирован.
        /// </summary>
        public const int JournalEventUserStateBlocked = JournalEventBase + 4;

        [ModuleAction("users", ModuleCustomer.PERM_MANAGEUSERS)]
        public virtual ActionResult Users(UserState? state = null)
        {
            using (var db = new CoreContext())
            {
                var usersQuery = state.HasValue ?
                                db.Users.Where(x => x.State == state.Value).OrderBy(x => x.name) :
                                db.Users.OrderBy(x => x.name);

                var usersCount = usersQuery.Count();

                var model = new Design.Model.AdminUsersManage()
                {
                    RequestedState = state,
                    DataCountAll = usersCount
                };

                return View("admin/AdminUsersManage.cshtml", model);
            }
        }

        [ModuleAction("users2", ModuleCustomer.PERM_MANAGEUSERS)]
        public virtual ActionResult Users2()
        {
            return Users(UserState.Active);
        }

        public virtual JsonResult UsersList(UserState? state = null, Universal.Pagination.PrimeUiDataTableSourceRequest requestOptions = null)
        {
            if (!ModelState.IsValid) throw new Exception("Некорректные параметры запроса.");

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadUncommitted }))
                {
                    using (var db = new CoreContext())
                    {
                        var query = state.HasValue ?
                                        db.Users.Where(x => x.State == state.Value) :
                                        db.Users;

                        var sorted = false;
                        if (requestOptions != null)
                        {
                            if (requestOptions.FilterFields != null)
                            {
                                foreach (var filter in requestOptions.FilterFields)
                                {
                                    switch (filter.FieldName)
                                    {
                                        case nameof(Core.Db.User.IdUser):
                                            foreach (var constraint in filter.Constraints)
                                            {
                                                if (!int.TryParse(constraint.Value, out var idUser)) throw new HandledException($"Некорректное значение фильтра для поля '{filter.FieldName}'.");
                                                switch (constraint.MatchType)
                                                {
                                                    case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.Contains:
                                                        query = query.Where(x => Convert.ToString(x.IdUser).Contains(idUser.ToString()));
                                                        break;

                                                    case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.StartsWith:
                                                        query = query.Where(x => Convert.ToString(x.IdUser).StartsWith(idUser.ToString()));
                                                        break;
                                                }
                                            }
                                            break;

                                        case nameof(Core.Db.User.Superuser):
                                            foreach (var constraint in filter.Constraints)
                                            {
                                                if (!int.TryParse(constraint.Value, out var superuser)) throw new HandledException($"Некорректное значение фильтра для поля '{filter.FieldName}'.");
                                                query = query.Where(x => x.Superuser == superuser);
                                            }
                                            break;

                                        case "Requisites":
                                            foreach (var constraint in filter.Constraints)
                                            {
                                                switch (constraint.MatchType)
                                                {
                                                    case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.Contains:
                                                        query = query.Where(x => ((x.name ?? "") + "." + (x.email ?? "") + "." + (x.phone ?? "")).Contains(constraint.Value));
                                                        break;

                                                    case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.StartsWith:
                                                        query = query.Where(x => ((x.name ?? "") + "." + (x.email ?? "") + "." + (x.phone ?? "")).StartsWith(constraint.Value));
                                                        break;
                                                }
                                            }
                                            break;

                                        case nameof(Core.Db.User.email):
                                            foreach (var constraint in filter.Constraints)
                                            {
                                                switch (constraint.MatchType)
                                                {
                                                    case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.Contains:
                                                        query = query.Where(x => x.email.Contains(constraint.Value));
                                                        break;

                                                    case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.StartsWith:
                                                        query = query.Where(x => x.email.StartsWith(constraint.Value));
                                                        break;
                                                }
                                            }
                                            break;

                                        case nameof(Core.Db.User.phone):
                                            foreach (var constraint in filter.Constraints)
                                            {
                                                switch (constraint.MatchType)
                                                {
                                                    case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.Contains:
                                                        query = query.Where(x => x.phone.Contains(constraint.Value));
                                                        break;

                                                    case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.StartsWith:
                                                        query = query.Where(x => x.phone.StartsWith(constraint.Value));
                                                        break;
                                                }
                                            }
                                            break;

                                        case nameof(Core.Db.User.name):
                                            foreach (var constraint in filter.Constraints)
                                            {
                                                switch (constraint.MatchType)
                                                {
                                                    case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.Contains:
                                                        query = query.Where(x => x.name.Contains(constraint.Value));
                                                        break;

                                                    case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.StartsWith:
                                                        query = query.Where(x => x.name.StartsWith(constraint.Value));
                                                        break;
                                                }
                                            }
                                            break;

                                        case nameof(Core.Db.User.CommentAdmin):
                                            foreach (var constraint in filter.Constraints)
                                            {
                                                switch (constraint.MatchType)
                                                {
                                                    case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.Contains:
                                                        query = query.Where(x => x.CommentAdmin.Contains(constraint.Value));
                                                        break;

                                                    case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.StartsWith:
                                                        query = query.Where(x => x.CommentAdmin.StartsWith(constraint.Value));
                                                        break;
                                                }
                                            }
                                            break;

                                        default:
                                            throw new HandledException($"Фильтр по полю '{filter.FieldName}' не поддерживается.");

                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(requestOptions.SortByFieldName))
                            {
                                switch (requestOptions.SortByFieldName)
                                {
                                    case nameof(Core.Db.User.IdUser):
                                        sorted = true;
                                        query = requestOptions.SortByAcsending ? query.OrderBy(x => x.IdUser) : query.OrderByDescending(x => x.IdUser);
                                        break;

                                    case "Requisites":
                                        sorted = true;
                                        query = requestOptions.SortByAcsending ?
                                            query.OrderBy(x => x.name).ThenBy(x => x.email).ThenBy(x => x.phone) :
                                            query.OrderByDescending(x => x.name).ThenByDescending(x => x.email).ThenByDescending(x => x.phone);
                                        break;

                                    case nameof(Core.Db.User.email):
                                        sorted = true;
                                        query = requestOptions.SortByAcsending ? query.OrderBy(x => x.email) : query.OrderByDescending(x => x.email);
                                        break;

                                    case nameof(Core.Db.User.phone):
                                        sorted = true;
                                        query = requestOptions.SortByAcsending ? query.OrderBy(x => x.phone) : query.OrderByDescending(x => x.phone);
                                        break;

                                    case nameof(Core.Db.User.name):
                                        sorted = true;
                                        query = requestOptions.SortByAcsending ? query.OrderBy(x => x.name) : query.OrderByDescending(x => x.name);
                                        break;

                                    case nameof(Core.Db.User.State):
                                        sorted = true;
                                        query = requestOptions.SortByAcsending ? query.OrderBy(x => x.State) : query.OrderByDescending(x => x.State);
                                        break;

                                    case nameof(Core.Db.User.Superuser):
                                        sorted = true;
                                        query = requestOptions.SortByAcsending ? query.OrderBy(x => x.Superuser) : query.OrderByDescending(x => x.Superuser);
                                        break;

                                    case nameof(Core.Db.User.CommentAdmin):
                                        sorted = true;
                                        query = requestOptions.SortByAcsending ? query.OrderBy(x => x.CommentAdmin) : query.OrderByDescending(x => x.CommentAdmin);
                                        break;
                                }
                            }
                        }

                        var dataAllCount = query.Count();

                        if (requestOptions != null)
                        {
                            if (!sorted) query = query.OrderBy(x => x.IdUser);

                            if (requestOptions.FirstRow > 0) query = query.Skip((int)requestOptions.FirstRow);
                            if (requestOptions.RowsLimit > 0) query = query.Take((int)requestOptions.RowsLimit);
                        }

                        var data = query.ToList();
                        return ReturnJson(true, null, new Design.Model.AdminUsersManage()
                        {
                            RequestedState = state,
                            DataCountAll = dataAllCount,
                            DataList = data
                        });
                    }
                }
            }
            catch (HandledException ex)
            {
                RegisterEventWithCode(HttpStatusCode.InternalServerError, "Ошибка при загрузке списка пользователей", null, ex);
                return ReturnJson(false, $"Ошибка при загрузке списка пользователей. {ex.Message}");
            }
            catch (Exception ex)
            {
                RegisterEventWithCode(HttpStatusCode.InternalServerError, "Неожиданная ошибка при загрузке списка пользователей", null, ex);
                return ReturnJson(false, "Неожиданная ошибка при загрузке списка пользователей");
            }
        }

        [ModuleAction("users_add", ModuleCustomer.PERM_MANAGEUSERS)]
        public ActionResult UserAdd()
        {
            return UserEdit(0);
        }

        [ModuleAction("users_edit", ModuleCustomer.PERM_MANAGEUSERS)]
        public virtual ActionResult UserEdit(int IdUser = 0)
        {
            using (var db = new CoreContext())
            {
                var data = IdUser != 0 ? db.Users.Where(x => x.IdUser == IdUser).FirstOrDefault() : new User();
                if (data == null) throw new KeyNotFoundException("Неправильно указан пользователь!");
                var history = AppCore.Get<JournalingManager>().GetJournalForItem(data);

                var model = new Design.Model.AdminUserEditForm()
                {
                    history = history.Result,
                    IsNeedToChangePassword = false,
                    User = data,
                    UserRoles = db.RoleUser
                                    .Where(x => x.IdUser == data.IdUser)
                                    .Select(x => x.IdRole)
                                    .ToList(),
                    Roles = db.Role
                                .OrderBy(x => x.NameRole)
                                .Select(x => new SelectListItem()
                                {
                                    Value = x.IdRole.ToString(),
                                    Text = x.NameRole
                                })
                                .ToList(),
                };

                return View("admin/AdminUserEditForm.cshtml", model);
            }
        }

        [ModuleAction("usersSave", ModuleCustomer.PERM_MANAGEUSERS)]
        public virtual JsonResult UserSave(int IdUser = 0, Model.AdminUserEdit model = null)
        {
            var result = JsonAnswer<int>();

            try
            {
                if (IdUser < 0) throw new Exception("Не указан пользователь!");
                else
                {
                    using (var db = new CoreContext())
                    {
                        int id = 0;

                        User data = null;
                        UserState oldState = 0;

                        if (IdUser > 0)
                        {
                            data = db.Users.Where(u => u.IdUser == IdUser).FirstOrDefault();
                            if (data == null) ModelState.AddModelError("IdUser", "Неправильно указан пользователь!");
                            else if (data.Superuser != 0 && !AppCore.GetUserContextManager().GetCurrentUserContext().IsSuperuser) ModelState.AddModelError("IdUser", "У вас нет прав на редактирование суперпользователей - это могут делать только другие суперпользователи!");
                            else
                            {
                                oldState = data.State;
                                id = IdUser;
                            }
                        }
                        else
                        {
                            data = new User()
                            {
                                salt = "",
                                //DateReg = DateTime.Now.Timestamp(),
                                // todo добавить запись о регистрации в журнал регистраций
                            };
                        }

                        var errors = new List<string>();

                        if (ModelState.ContainsKeyCorrect("User.email")) data.email = model.User.email?.ToLower();
                        if (ModelState.ContainsKeyCorrect("User.phone")) data.phone = model.User.phone;

                        if (ModelState.ContainsKeyCorrect("User.name")) data.name = model.User.name;
                        if (Request.Form.HasKey("login") && !string.IsNullOrEmpty(Request.Form["login"]))
                        {
                            if (!Request.Form["login"].isOneStringTextOnly()
                                // todo переработать этот участок в нормальную модель || DataManager.check(Request.Form["login"])
                                ) errors.Add("Некорректный ввод поля login!");
                            else data.name = Request.Form["login"];
                        }

                        if (ModelState.ContainsKeyCorrect("User.name")) data.name = model.User.name;
                        if (ModelState.ContainsKeyCorrect("User.about")) data.about = model.User.about;

                        if (ModelState.ContainsKeyCorrect("User.Superuser"))
                        {
                            if (!AppCore.GetUserContextManager().GetCurrentUserContext().IsSuperuser) errors.Add("Недостаточно прав для установки или снятия признака суперпользователя!");
                            data.Superuser = (byte)(model.User.Superuser == 0 ? 0 : 1);
                        }

                        if (ModelState.ContainsKeyCorrect("User.State"))
                        {
                            switch (model.User.State)
                            {
                                case UserState.Active:
                                case UserState.RegisterNeedConfirmation:
                                case UserState.RegisterWaitForModerate:
                                case UserState.RegisterDecline:
                                case UserState.Disabled:
                                    data.State = model.User.State;
                                    break;

                                default:
                                    ModelState.AddModelError("User.State", "Неизвестное состояние пользователя.");
                                    break;
                            }
                        }

                        if (ModelState.ContainsKeyCorrect(nameof(model.IsNeedToChangePassword)) && ModelState.ContainsKeyCorrect(nameof(model.User) + "." + nameof(model.User.password)))
                        {
                            if (data.IdUser > 0 && model.IsNeedToChangePassword)
                                data.password = UsersExtensions.hashPassword(model.User.password);
                            else if (data.IdUser == 0)
                                data.password = UsersExtensions.hashPassword(model.User.password);
                        }

                        if (ModelState.ContainsKeyCorrect("User.Comment")) data.Comment = model.User.Comment;
                        if (ModelState.ContainsKeyCorrect("User.CommentAdmin")) data.CommentAdmin = model.User.CommentAdmin;
                        if (Request.Form.HasKey("adminComment") && !string.IsNullOrEmpty(Request.Form["adminComment"]))
                        {
                            if (!Request.Form["adminComment"].isOneStringTextOnly()
                                // todo переработать этот участок в нормальную модель || DataManager.check(Request.Form["adminComment"])
                                ) errors.Add("Некорректный ввод комментария администратора!");
                            else data.CommentAdmin = Request.Form["adminComment"];
                        }

                        result.Message = errors.Count > 0 ? " - " + string.Join("\r\n - ", errors) : "";

                        if (errors.Count == 0 && ModelState.IsValid)
                        {
                            data.Fields.CopyValuesFrom(model.User.Fields);
                            data.DateChangeBase = DateTime.Now;
                            data.IdUserChange = AppCore.GetUserContextManager().GetCurrentUserContext().IdUser;

                            using (var trans = new TransactionScope())
                            {
                                if (data.IdUser == 0) db.Users.Add(data);

                                if (db.SaveChanges<User>() > 0)
                                {
                                    result.Message = "Сохранение данных прошло успешно!";
                                    result.Success = true;

                                    Module.RegisterEventForItem(data, EventType.Info, JournalEventUserEdit, "Редактирование данных", $"Пользователь №{data.IdUser} '" + data.ToString() + "'");

                                    if (result.Success)
                                    {
                                        {
                                            var rolesMustHave = new List<int>(model.UserRoles ?? new List<int>());
                                            db.RoleUser.RemoveRange(db.RoleUser.Where(x => x.IdUser == data.IdUser));
                                            rolesMustHave.ForEach(x => db.RoleUser.Add(new RoleUser()
                                            {
                                                IdRole = x,
                                                IdUser = data.IdUser,
                                                IdUserChange = AppCore.GetUserContextManager().GetCurrentUserContext().IdUser,
                                                DateChange = DateTime.Now.Timestamp()
                                            }));

                                            db.SaveChanges<RoleUser>();
                                        }


                                        /*
                                         * todo рассылка на мыло и по телефону
                                         * */
                                        if (oldState == UserState.RegisterWaitForModerate && data.State == UserState.Active)
                                        {
                                            this.assign("login", Request.Form["email"]);
                                            this.assign("message", "Ваша заявка была одобрена администратором, вы можете зайти на сайт, используя логин и пароль, указанные при регистрации!");

                                            AppCore.Get<EmailService>().SendMailFromSite(
                                                data.ToString(),
                                                data.email,
                                                "Успешная регистрация на сайте",
                                                this.ViewString("Register/register_mail2.cshtml"),
                                                ContentType.Html
                                            );

                                            Module.RegisterEventForItem(data, EventType.Info, JournalEventUserStateAccept, "Заявка одобрена", "Пользователь №" + data.IdUser + " '" + data.ToString() + "'");
                                        }
                                        if (oldState == UserState.RegisterWaitForModerate && data.State == UserState.RegisterDecline)
                                        {
                                            var message = ".";

                                            //Если администратор указал комментарий к отклонению заявки
                                            if (!string.IsNullOrEmpty(data.CommentAdmin))
                                            {
                                                message = " по следующей причине: " + data.CommentAdmin;
                                                this.assign("comment", data.CommentAdmin);
                                            }

                                            this.assign("login", data.email);
                                            this.assign("message", "Ваша заявка была отклонена администратором" + message);

                                            AppCore.Get<EmailService>().SendMailFromSite(
                                                data.ToString(),
                                                data.email,
                                                "Регистрация на сайте отклонена",
                                                this.ViewString("Register/register_mail_decline.cshtml"),
                                                ContentType.Html
                                            );

                                            Module.RegisterEventForItem(data, EventType.Info, JournalEventUserStateDecline, "Заявка отклонена", "Пользователь №" + data.IdUser + " '" + data.ToString() + "'. Заявка отклонена администратором" + message);
                                        }
                                        if (oldState != data.State && data.State == UserState.Disabled)
                                        {
                                            var message = ".";

                                            //Если администратор указал комментарий к отключению заявки
                                            if (Request.Form.HasKey("adminComment") && !string.IsNullOrEmpty(Request.Form["adminComment"]))
                                            {
                                                message = " по следующей причине: " + Request.Form["adminComment"];
                                                this.assign("comment", Request.Form["adminComment"]);
                                            }
                                            if (Request.Form.HasKey("CommentAdmin") && !string.IsNullOrEmpty(Request.Form["CommentAdmin"]))
                                            {
                                                message = " по следующей причине: " + Request.Form["CommentAdmin"];
                                                this.assign("comment", Request.Form["CommentAdmin"]);
                                            }

                                            this.assign("login", data.email);
                                            this.assign("message", "Ваш аккаунт заблокирован администратором" + message);
                                            AppCore.Get<EmailService>().SendMailFromSite(
                                                data.ToString(),
                                                data.email,
                                                "Аккаунт заблокирован",
                                                this.ViewString("Register/register_mail_ban.cshtml"),
                                                ContentType.Html
                                            );

                                            Module.RegisterEventForItem(data, EventType.Info, JournalEventUserStateBlocked, "Аккаунт заблокирован", "Пользователь №" + data.IdUser + " '" + data.ToString() + "'. Аккаунт заблокирован" + message);
                                        }

                                    }

                                    result.Data = data.IdUser;

                                    trans.Complete();
                                }
                                else result.Message = "Сохранение данных провалилось!";
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { result.FromException(ex); }

            return ReturnJson(result);
        }

        [ModuleAction("users_delete", ModuleCustomer.PERM_MANAGEUSERS)]
        public virtual JsonResult UserDelete(int IdUser = 0)
        {
            var result = JsonAnswer();
            try
            {
                if (IdUser == 0) result.Message = "Не указан пользователь!";
                else if (IdUser == AppCore.GetUserContextManager().GetCurrentUserContext().IdUser) result.Message = "Нельзя удалять себя самого!";
                else
                {
                    using (var db = new CoreContext())
                    {
                        var data = db.Users.Where(x => x.IdUser == IdUser).FirstOrDefault();
                        if (data == null) result.Message = "Неправильно указан пользователь!";
                        else
                        {
                            db.Users.Remove(data);
                            if (db.SaveChanges(data.GetType()) > 0)
                            {
                                result.Message = "Удаление прошло успешно!";
                                result.Success = true;

                                //SystemHistoryManager.register($this, "Пользователь №".$data['id"]." '".$data['name"]."'", "Пользователь '".$data['email"]."' удален", "User_$IdUser");
                            }
                            else result.Message = "Не удалось удалить пользователя!";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }

            return ReturnJson(result);
        }

        [ModuleAction("userAs", ModuleCustomer.PERM_MANAGEUSERS)]
        public virtual RedirectResult UserShowAs(int IdUser = 0)
        {
            var result = "";
            if (IdUser < 0) result = "Не указан пользователь!";
            else if (!AppCore.GetUserContextManager().GetCurrentUserContext().IsSuperuser) result = "Нельзя делать это без прав суперпользователя!";
            else
            {
                using (var db = new CoreContext())
                {
                    var data = db.Users.Where(u => u.IdUser == IdUser).FirstOrDefault();
                    if (data == null) throw new Exception("Неправильно указан пользователь!");

                    Response.SetCookie(new System.Web.HttpCookie("LogonSuperuserAs", IdUser.ToString()) { Expires = DateTime.Now.AddDays(365), Domain = "/" });
                    return new RedirectResult("/");
                }
            }

            throw new Exception(result);
        }

        [ModuleAction("rolesManage", ModuleCustomer.PERM_MANAGEROLES)]
        public virtual ActionResult RolesManage()
        {
            var model = new Model.AdminRolesManage();
            using (var db = new CoreContext())
            {
                var permsQuery = (from p in db.RolePermission
                                  select new { p.IdRole, p.IdModule, p.Permission }
                                  ).Distinct();

                var perms = permsQuery.
                             ToList().
                             GroupBy(x => x.IdRole).
                             ToDictionary(x => x.Key,
                                          x => x.Select(p => string.Format("{0};{1}", p.IdModule, p.Permission)).ToList());


                model.Roles = db.Role
                                    .OrderBy(x => x.NameRole)
                                    .ToDictionary(x => x.IdRole,
                                                  x => new Model.AdminRoleEdit(x)
                                                  {
                                                      Permissions = perms.ContainsKey(x.IdRole) ? perms[x.IdRole] : new List<string>()
                                                  });

                var mperms = new List<SelectListItem>();
                foreach (var module in AppCore.GetModulesManager().GetModules().OrderBy(x => x.Caption))
                {
                    var gr = new SelectListGroup() { Name = module.Caption };
                    mperms.AddRange(module.GetPermissions().OrderBy(x => x.Caption).Select(x => new SelectListItem()
                    {
                        Group = gr,
                        Value = string.Format("{0};{1}", module.ID, x.Key),
                        Text = x.Caption
                    }));
                }
                model.ModulesPermissions = mperms;
            }
            return View("admin/admin_customer_rolesManage.cshtml", model);
        }

        [ModuleAction("roleSave", ModuleCustomer.PERM_MANAGEROLES)]
        public virtual JsonResult RoleSave(Model.AdminRoleEdit model = null)
        {
            var result = JsonAnswer<Model.AdminRoleEdit>();

            try
            {
                if (model == null) throw new Exception("Из формы не были отправлены данные.");

                using (var db = new CoreContext())
                {
                    Role data = null;
                    if (model.IdRole > 0)
                    {
                        data = db.Role.Where(x => x.IdRole == model.IdRole).FirstOrDefault();
                        if (data == null) ModelState.AddModelError(nameof(model.IdRole), "Роль с номером {0} не найдена", model.IdRole);
                    }
                    else
                    {
                        data = new Role()
                        {
                            IdUserCreate = AppCore.GetUserContextManager().GetCurrentUserContext().IdUser,
                            DateCreate = DateTime.Now.Timestamp()
                        };
                    }

                    if (ModelState.IsValid)
                    {
                        if (ModelState.ContainsKeyCorrect(nameof(model.NameRole)))
                            if (db.Role.Where(x => x.IdRole != model.IdRole && x.NameRole == model.NameRole).Count() > 0)
                                ModelState.AddModelError(nameof(model.NameRole), "Роль с именем '{0}' уже существует.", model.NameRole);

                        if (ModelState.ContainsKeyCorrect(nameof(model.Permissions)))
                        {
                            if (model.Permissions.Count > 500) ModelState.AddModelError(nameof(model.Permissions), "Количество разрешающих пунктов в роли не может быть больше 500");
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        data.NameRole = model.NameRole;
                        data.IsHidden = model.IsHidden;
                        data.IdUserChange = AppCore.GetUserContextManager().GetCurrentUserContext().IdUser;
                        data.DateChange = DateTime.Now.Timestamp();

                        if (data.IdRole == 0) db.Role.Add(data);

                        using (var trans = new TransactionScope())
                        {
                            if (db.SaveChanges(data.GetType()) > 0)
                            {
                                model.IdRole = data.IdRole;

                                db.RolePermission.RemoveRange(db.RolePermission.Where(x => x.IdRole == data.IdRole));

                                if (ModelState.ContainsKeyCorrect(nameof(model.Permissions)))
                                {
                                    model.Permissions.ForEach(x =>
                                    {
                                        if (x.IndexOf(';') > 0)
                                        {
                                            var d = x.Split(new char[] { ';' }, 2);
                                            if (d.Length == 2)
                                            {
                                                int moduleID = 0;
                                                if (int.TryParse(d[0], out moduleID))
                                                {
                                                    db.RolePermission.Add(new RolePermission()
                                                    {
                                                        IdModule = moduleID,
                                                        IdRole = data.IdRole,
                                                        Permission = d[1],
                                                        IdUserChange = AppCore.GetUserContextManager().GetCurrentUserContext().IdUser,
                                                        DateChange = DateTime.Now.Timestamp()
                                                    });

                                                }
                                            }
                                        }
                                    });

                                    if (db.SaveChanges<RolePermission>() == 0)
                                        throw new Exception($"Возникли ошибки при сохранении разрешений для роли '{data.NameRole}'");
                                }

                                Module.RegisterEventForItem(data, EventType.Info, 0, "Роль обновлена", $"Роль №{data.IdRole} '{data.NameRole}'");

                                trans.Complete();

                                result.Message = "Сохранение роли прошло успешно!";
                                result.Success = true;

                                result.Data = new Model.AdminRoleEdit(data) { Permissions = model.Permissions };
                            }
                            else result.Message = "Не удалось сохранить роль.";
                        }
                    }
                }
            }
            catch (Exception ex) { result.FromException(ex); }

            return ReturnJson(result);
        }

        [ModuleAction("roleDelete", ModuleCustomer.PERM_MANAGEROLES)]
        public virtual JsonResult RoleDelete(int IdRole = 0)
        {
            var result = JsonAnswer();

            try
            {
                using (var db = new CoreContext())
                {
                    var data = db.Role.Where(x => x.IdRole == IdRole).FirstOrDefault();
                    if (data == null) ModelState.AddModelError(nameof(IdRole), "Роль с номером {0} не найдена", IdRole);

                    if (ModelState.IsValid)
                    {
                        using (var scope = new TransactionScope())
                        {
                            db.Role.Remove(data);
                            db.RolePermission.RemoveRange(db.RolePermission.Where(x => x.IdRole == IdRole));

                            if (db.SaveChanges() > 0)
                            {
                                result.Message = "Удаление роли прошло успешно!";
                                result.Success = true;

                                Module.RegisterEventForItem(data, EventType.Info, 0, "Роль удалена", $"Роль №{data.IdRole} '{data.NameRole}'");

                                scope.Complete();
                            }
                            else result.Message = "Не удалось удалить роль.";
                        }
                    }
                }
            }
            catch (Exception ex) { result.FromException(ex); }

            return ReturnJson(result);
        }

        [ModuleAction("rolesDelegate", ModuleCustomer.PERM_MANAGEROLES)]
        public virtual ActionResult RolesDelegate(bool? isShowHidden = null)
        {
            var model = new Model.AdminRolesDelegate();
            using (var db = new CoreContext())
            {
                model.Roles = db.Role.OrderBy(x => x.NameRole).ToList();
                model.Users = db.Users.ToList().OrderBy(x => x.ToString()).ToList();

                var queryRoleUsers = (from user in db.Users
                         join roleUser in db.RoleUser on user.IdUser equals roleUser.IdUser
                         join role in db.Role on roleUser.IdRole equals role.IdRole
                         select new { roleUser.IdRole, roleUser.IdUser }).Distinct();

                model.RolesUser = queryRoleUsers.ToList().GroupBy(x => x.IdUser, x => x.IdRole).ToDictionary(x => x.Key, x => x.ToList());

            }
            return View("admin/admin_customer_rolesDelegate.cshtml", model);
        }

        [ModuleAction("rolesDelegateSave", ModuleCustomer.PERM_MANAGEROLES)]
        public virtual JsonResult RolesDelegateSave([Bind(Prefix = "Roles")] Dictionary<int, List<int>> model = null)
        {
            var result = JsonAnswer();

            try
            {
                using (var db = new CoreContext())
                using (var scope = db.CreateScope())
                {
                    var hiddenRoleList = db.Role.Where(x => x.IsHidden).Select(x => x.IdRole).ToList();
                    var hiddenRoleUserQuery = from role in db.Role
                                           join roleUser in db.RoleUser on role.IdRole equals roleUser.IdRole
                                           where role.IsHidden
                                           select roleUser;
                    var hiddenRoleUserList = hiddenRoleUserQuery.ToList();

                    db.RoleUser.RemoveRange(db.RoleUser);

                    var saveList = hiddenRoleUserList.Select(x => new RoleUser()
                    {
                        IdRole = x.IdRole,
                        IdUser = x.IdUser,
                        IdUserChange = x.IdUserChange,
                        DateChange = x.DateChange
                    }).ToList();

                    if (model != null)
                    {
                        foreach (var user in model)
                        {
                            user.Value?.
                                Where(x => !hiddenRoleList.Contains(x)).
                                ForEach(x => saveList.Add(new RoleUser()
                                {
                                    IdRole = x,
                                    IdUser = user.Key,
                                    IdUserChange = AppCore.GetUserContextManager().GetCurrentUserContext().IdUser,
                                    DateChange = DateTime.Now.Timestamp()
                                }));
                        }
                    }

                    db.RoleUser.AddRange(saveList.ToArray());
                    db.SaveChanges();

                    scope.Complete();
                    result.Success = true;
                    result.Message = "Права сохранены";
                }
            }
            catch (Exception ex) { result.FromException(ex); }

            return ReturnJson(result);
        }
    }
}
