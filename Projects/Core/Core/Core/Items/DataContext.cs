using OnUtils.Data;

namespace OnXap.Core.Items
{
    class DataContext : Db.CoreContextBase
    {
        public IRepository<Db.ItemLink> ItemLink { get; set; }
        public IRepository<Db.ItemParent> ItemParent { get; set; }
        public IRepository<Db.ItemType> ItemType { get; set; }

    }
}
