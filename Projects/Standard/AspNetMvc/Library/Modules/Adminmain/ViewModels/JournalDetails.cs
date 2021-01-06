using System.Collections.Generic;

namespace OnXap.Modules.Adminmain.ViewModels
{
    using Journaling.Model;

    public class JournalDetails
    {
        public int IdJournal { get; set; }

        public string NameJournal { get; set; }

        public int JournalDataCountAll { get; set; }

        public List<JournalData> JournalData { get; set; }
    }
}