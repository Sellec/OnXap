using Microsoft.EntityFrameworkCore;

namespace OnXap.Modules.Materials.DB
{
    using Core.Db;

    public class DataLayerContext : CoreContextBase
    {
        public virtual DbSet<Page> Pages { get; set; }
        public virtual DbSet<PageCategory> PageCategories { get; set; }
        public virtual DbSet<News> News { get; set; }
    }

}
