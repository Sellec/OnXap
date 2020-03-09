using OnUtils.Data;

namespace OnXap.Modules.Materials.DB
{
    using Core.Db;

    public class DataLayerContext : CoreContextBase
    {
        public virtual IRepository<Page> Pages { get; set; }
        public virtual IRepository<PageCategory> PageCategories { get; set; }

        public virtual IRepository<News> News { get; set; }
    }

}
