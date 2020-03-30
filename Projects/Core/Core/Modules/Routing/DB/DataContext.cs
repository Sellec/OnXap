using Microsoft.EntityFrameworkCore;

namespace OnXap.Modules.Routing.Db
{
    using Core.Db;

    public class DataContext : CoreContextBase
    {
        public DbSet<Routing> Routing { get; set; }
    }
}
