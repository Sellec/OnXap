using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace OnXap.Modules.Auth
{
    using Core.Db;
    using Core.Modules;
    using Users;

    /// <summary>
    /// Модуль авторизации.
    /// </summary>
    [ModuleCore("Авторизация", DefaultUrlName = "Auth")]
    public abstract class ModuleAuth : ModuleCore<ModuleAuth>
    {
        /// <summary>
        /// Указывает, требуется ли на сайте суперпользователь. Если на данный момент активного суперпользователя нет (нет учетки с пометкой суперпользователя или все суперпользователи заблокированы), то 
        /// модуль регистрации пометит ближайшего зарегистрированного пользователя как суперпользователя и сразу сделает активным.
        /// </summary>
        /// <returns></returns>
        public bool IsSuperuserNeeded()
        {
            var getSystemUser = AppCore.GetUserContextManager().GetSystemUserContext();

            using (var db = new CoreContext())
            {
                var query = db.Users.AsNoTracking().Where(x => x.Superuser != 0 && x.Block == 0 && x.State == UserState.Active);
                if (getSystemUser != null) query = query.Where(x => x.IdUser != getSystemUser.IdUser);
                return query.Count() == 0;
            }
        }

        /// <summary>
        /// Указывает, что на сайте нет ни одного пользователя и требуется немедленная регистрация.
        /// </summary>
        /// <returns></returns>
        public bool IsNeededAnyUserToRegister()
        {
            var getSystemUser = AppCore.GetUserContextManager().GetSystemUserContext();

            using (var db = new CoreContext())
            {
                var query = db.Users.AsNoTracking().AsQueryable();
                if (getSystemUser != null) query = query.Where(x => x.IdUser != getSystemUser.IdUser);

                return query.Count() == 0;
            }
        }

        /// <summary>
        /// Запоминает адрес <paramref name="requestedAddress"/>, запрошенный пользователем, ассоциированным с текущим активным контекстом (см. <see cref="UserContextManagerBase.GetCurrentUserContext"/>).
        /// </summary>
        public void RememberUserContextRequestedAddressWhenRedirectedToAuthorization(Uri requestedAddress)
        {
            RememberUserContextRequestedAddressWhenRedirectedToAuthorization(AppCore.GetUserContextManager().GetCurrentUserContext(), requestedAddress);
        }

        /// <summary>
        /// Запоминает адрес <paramref name="requestedAddress"/>, запрошенный пользователем, ассоциированным с указанным контекстом <paramref name="userContext"/>.
        /// </summary>
        public virtual void RememberUserContextRequestedAddressWhenRedirectedToAuthorization(IUserContext userContext, Uri requestedAddress)
        {

        }

        /// <summary>
        /// Возвращает адрес, запомненный модулем во время последнего вызова <see cref="RememberUserContextRequestedAddressWhenRedirectedToAuthorization(IUserContext, Uri)"/> для пользователя, ассоциированного с текущим активным контекстом (см. <see cref="UserContextManagerBase.GetCurrentUserContext"/>).
        /// </summary>
        public Uri GetRememberedUserContextRequestedAddressWhenRedirectedToAuthorization()
        {
            return GetRememberedUserContextRequestedAddressWhenRedirectedToAuthorization(AppCore.GetUserContextManager().GetCurrentUserContext());
        }

        /// <summary>
        /// Возвращает адрес, запомненный модулем во время последнего вызова <see cref="RememberUserContextRequestedAddressWhenRedirectedToAuthorization(IUserContext, Uri)"/> для пользователя, ассоциированного с указанным контекстом <paramref name="userContext"/>.
        /// </summary>
        public virtual Uri GetRememberedUserContextRequestedAddressWhenRedirectedToAuthorization(IUserContext userContext)
        {
            return null;
        }

    }
}
