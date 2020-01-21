using System.Collections.Generic;

namespace OnXap.Modules.Adminmain.Design.Model
{
    using Journaling.Model;

    public class JournalDetails
    {
        public JournalInfo JournalName { get; set; }

        public int JournalDataCountAll { get; set; }

        public List<JournalData> JournalData { get; set; }
    }
}