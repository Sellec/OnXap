using System;

namespace OnXap.Modules.Adminmain.ViewModels
{
    using Journaling;
    using Journaling.Model;

    public class JournalsList
    {
        public JournalInfo JournalName { get; set; }

        public int EventsCount { get; set; }

        public DateTime? LatestEventDate { get; set; }

        public EventType? LatestEventType { get; set; }
    }
}
