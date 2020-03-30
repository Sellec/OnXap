using Microsoft.EntityFrameworkCore;

namespace OnXap.Core.Items.Db
{
    using Core.Db;

    class DataContext : CoreContextBase
    {
        public DbSet<ItemLink> ItemLink { get; set; }
        public DbSet<ItemParent> ItemParent { get; set; }
        public DbSet<ItemType> ItemType { get; set; }
    }
}
