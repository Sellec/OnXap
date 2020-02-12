using OnUtils.Data;

namespace OnXap.Modules.Auth.Db
{
    using Core.Db;

    class DataContext : CoreContext
    {
        public IRepository<UserPasswordRecovery> UserPasswordRecovery { get; }

    }
}
