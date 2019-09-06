using System.Collections.Generic;

namespace OnXap.Modules.Auth.Design.Model
{
    using Core.DB;

    public class ModuleSettings : Auth.Model.Configuration
    {
        public List<UserLogHistoryEventType> EventTypes { get; set; }
    }
}
