#pragma warning disable CS1591
namespace OnXap.Journaling.DB
{
    public class QueryJournalData
    {
        public JournalDAO JournalData { get; set; }

        public JournalNameDAO JournalName { get; set; }

        public Core.Db.User User { get; set; }
    }
}
