using OnUtils.Data;

#pragma warning disable CS1591
namespace OnXap.Journaling.DB
{
    using Core.Db;
    using Core.Items.Db;

    public class DataContext : CoreContext
    {
        public IRepository<ItemLink> ItemLink { get; }
        public IRepository<JournalDAO> Journal { get; }
        public IRepository<JournalNameDAO> JournalName { get; }
    }
}
