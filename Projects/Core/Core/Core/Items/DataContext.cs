using OnUtils.Data;

namespace OnXap.Core.Items
{
    class DataContext : DB.CoreContextBase
    {
        public IRepository<DB.ItemLink> ItemLink { get; set; }
        public IRepository<DB.ItemParent> ItemParent { get; set; }
        public IRepository<DB.ItemType> ItemType { get; set; }

    }
}
