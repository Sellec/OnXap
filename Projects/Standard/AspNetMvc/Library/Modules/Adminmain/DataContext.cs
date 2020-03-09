using Microsoft.EntityFrameworkCore;
using OnUtils.Data;

namespace OnXap.Modules.Adminmain
{
    using Core.Db;
    using Routing.Db;

    /// <summary>
    /// Контекст для доступа к данным.
    /// </summary>
    public class DataContext : CoreContextBase
    {
        /// <summary>
        /// </summary>
        public DbSet<ModuleConfig> ConfigModules { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<Role> Role { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<Routing> Routes { get; set; }
    }
}