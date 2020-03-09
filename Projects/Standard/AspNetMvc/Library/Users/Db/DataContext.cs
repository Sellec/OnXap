using Microsoft.EntityFrameworkCore;
using OnUtils.Data;

namespace OnXap.Users.Db
{
    using Core.Db;

    class DataContext : CoreContext
    {
        public DbSet<UserEntity> UserEntity { get; set; }

    }
}
