using OnUtils.Data;

#pragma warning disable CS1591
namespace OnXap.Journaling.DB
{
    public class DataContext : Core.DB.CoreContext
    {
        public IRepository<JournalDAO> Journal { get; }

        public IRepository<JournalNameDAO> JournalName { get; }
    }
}
