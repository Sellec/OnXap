using System;
using System.Collections.Generic;
using System.Text;

namespace OnXap.Plugins.Communication
{
    using Core.Configuration;

    public class ModuleConfiguration : ModuleConfiguration<ModuleCommunication>
    {
        public string AdminUserName { get; set; }
    }
}
