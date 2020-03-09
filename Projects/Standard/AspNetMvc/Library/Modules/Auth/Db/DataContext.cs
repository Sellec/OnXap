using Microsoft.EntityFrameworkCore;
using OnUtils.Data;

namespace OnXap.Modules.Auth.Db
{
    using Core.Db;

    class DataContext : CoreContext
    {
        public DbSet<UserPasswordRecovery> UserPasswordRecovery { get; set; }

    }
}
