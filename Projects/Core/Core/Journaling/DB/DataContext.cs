using OnUtils.Data;

#pragma warning disable CS1591
namespace OnXap.Journaling.DB
{
    public class DataContext : Core.Db.CoreContext
    {
        public IRepository<Core.Db.ItemLink> ItemLink { get; }
        public IRepository<JournalDAO> Journal { get; }
        public IRepository<JournalNameDAO> JournalName { get; }
    }
}
