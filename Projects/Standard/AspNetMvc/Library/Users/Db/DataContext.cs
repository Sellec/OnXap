using OnUtils.Data;

namespace OnXap.Users.Db
{
    using Core.Db;

    class DataContext : CoreContext
    {
        public IRepository<UserEntity> UserEntity { get; }

    }
}
