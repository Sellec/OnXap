using OnUtils.Data;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS1591
namespace OnXap.Journaling.DB
{
    using Core.Db;
    using Core.Items.Db;

    public class DataContext : CoreContext
    {
        public DbSet<ItemLink> ItemLink { get; set; }
        public DbSet<JournalDAO> Journal { get; set; }
        public DbSet<JournalNameDAO> JournalName { get; set; }
    }
}
