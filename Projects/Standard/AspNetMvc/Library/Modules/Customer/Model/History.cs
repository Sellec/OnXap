using System;
using System.Collections.Generic;

namespace OnXap.Modules.Customer.Model
{
    using Journaling.Model;

    public class History
    {
        public DateTime DateStart;
        public DateTime DateEnd;
        public IList<JournalData> Records;
    }
}