using System;

namespace OnXap.Modules.Customer
{
    using Core.Db;
    using Core.Items;
    using Core.Modules;

    /// <summary>
    /// Модуль для управления пользователями и личным кабинетом.
    /// </summary>
    [ModuleCore("Личный кабинет", DefaultUrlName = "Customer")]
    public abstract class ModuleCustomer : ModuleCore<ModuleCustomer>
    {
        /// <summary>
        /// Управление пользователями
        /// </summary>
        public const string PERM_MANAGEUSERS = "manage_users";

        /// <summary>
        /// Управление ролями
        /// </summary>
        public const string PERM_MANAGEROLES = "manage_roles";

        /// <summary>
        /// Просмотр истории
        /// </summary>
        public const string PERM_VIEWHISTORY = "history";

        /// <summary>
        /// См. <see cref="ModuleCore.OnModuleStarting"/>.
        /// </summary>
        protected sealed override void OnModuleStarting()
        {
            RegisterPermission(PERM_MANAGEUSERS, "Управление пользователями");
            RegisterPermission(PERM_MANAGEROLES, "Управление ролями");
            RegisterPermission(PERM_VIEWHISTORY, "Просмотр истории");

            AppCore.Get<ItemsManager>().RegisterModuleItemType<User, ModuleCustomer>(this);
        }

        /// <summary>
        /// В классе-наследнике должен регистрировать платформозависимые валидаторы моделей.
        /// </summary>
        protected abstract void RegisterModelValidators();

        /// <summary>
        /// См. <see cref="ModuleCore.GenerateLink(ItemBase)"/>.
        /// </summary>
        public sealed override Uri GenerateLink(ItemBase item)
        {
            if (item is User) return new Uri(AppCore.ServerUrl, $"{UrlName}/user/{item.IdBase}");
            //else if (item is Register.Model.Register) return new Uri(ApplicationCore.Instance.ServerUrl, $"{Name}/user/{item.ID}");
            //return base.GenerateLink(item);

            return null;
        }

        /// <summary>
        /// Проверяет, может ли текущий пользователь редактировать профиль пользователя с идентификатором <paramref name="IdUser"/>. Генерирует исключение с текстом ошибки, если прав недостаточно.
        /// </summary>
        public virtual void CheckPermissionToEditOtherUser(int IdUser)
        {
            var context = AppCore.GetUserContextManager().GetCurrentUserContext();

            if (IdUser == context.IdUser) return;

            if (!context.IsSuperuser) throw new Exception("Недостаточно прав на редактирование других пользователей.");
        }
    }
}
