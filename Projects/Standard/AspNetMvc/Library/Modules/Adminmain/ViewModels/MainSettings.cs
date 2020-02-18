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

        public List<SelectListItem> ModulesList { get; set; } = new List<SelectListItem>();
    }
}

