namespace OnXap.Modules.Adminmain.Model
{
    using JournalingA = Journaling;

    static class JournalQueries
    {
        public class QueryJournalData : JournalingA.DB.QueryJournalData
        {
            public int Count { get; set; }
        }

        public class JournalData : JournalingA.Model.JournalData
        {
            public int Count { get; set; }
        }


    }
}
