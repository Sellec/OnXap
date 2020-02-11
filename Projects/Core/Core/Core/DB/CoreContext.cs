using OnUtils.Data;
using OnUtils.Data.UnitOfWork;

namespace OnXap.Core.Db
{
    using Languages.DB;

    /// <summary>
    /// Ѕазовый контекст веб-приложени€, корректно определ€ющий строку подключени€.
    /// </summary>
    public class CoreContextBase : UnitOfWorkBase
    {
        /// <summary>
        /// </summary>
        protected sealed override void OnModelCreating(IModelAccessor modelAccessor)
        {
            modelAccessor.ConnectionString = ConnectionString;
            OnModelCreatingCustom(modelAccessor);
            modelAccessor.ConnectionString = ConnectionString;
        }

        /// <summary>
        /// —м. <see cref="UnitOfWorkBase.OnModelCreating(IModelAccessor)"/>.
        /// </summary>
        protected virtual void OnModelCreatingCustom(IModelAccessor modelAccessor)
        {
        }

        internal static string ConnectionString { get; set; }
    }

    /// <summary>
    /// ќсновной контекст, содержащий все сущности €дра. 
    /// </summary>
    /// <seealso cref="CoreContextBase"/>
    public class CoreContext : CoreContextBase
    {
#pragma warning disable CS1591 // todo внести комментарии.
        public IRepository<ModuleConfig> Module { get; }

        public IRepository<ItemType> ItemType { get; }
        public IRepository<Language> Language { get; }
        public IRepository<Sessions> Sessions { get; }

        public IRepository<PasswordRemember> PasswordRemember { get; }

        public IRepository<UserEntity> UserEntity { get; }
        public IRepository<UserLogHistory> UserLogHistory { get; }
        public IRepository<UserLogHistoryEventType> UserLogHistoryEventType { get; }
        public IRepository<User> Users { get; }

        public IRepository<Role> Role { get; }
        public IRepository<RoleUser> RoleUser { get; }
        public IRepository<RolePermission> RolePermission { get; }
    }
}
