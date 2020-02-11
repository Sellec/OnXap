using OnUtils.Data;

namespace OnXap.Core.Items.Db
{
    using Core.Db;

    class DataContext : CoreContextBase
    {
        public IRepository<ItemLink> ItemLink { get; set; }
        public IRepository<ItemParent> ItemParent { get; set; }
        public IRepository<ItemType> ItemType { get; set; }

    }
}
