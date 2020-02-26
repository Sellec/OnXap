using System.Collections.Generic;
using System.Web.Mvc;

namespace OnXap.Modules.Adminmain.ViewModels
{
    using Core.Configuration;
    using Core.Db;
    using WebCoreModule;

    public class MainSettings : Model.MainSettingsSave
    {
        public MainSettings(CoreConfiguration appCoreConfiguration, WebCoreConfiguration webCoreConfiguration) : base(appCoreConfiguration, webCoreConfiguration)
        {
        }

        public List<Role> Roles { get; set; }
        public List<MainSettingsModule> Modules { get; set; }
    }

    public class MainSettingsModule
    {
        public int Id { get; set; }
        public string Caption { get; set; }
    }
}

