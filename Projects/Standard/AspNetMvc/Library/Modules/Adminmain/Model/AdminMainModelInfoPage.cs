﻿using System.Collections.Generic;
using System.Web.Mvc;

namespace OnXap.Modules.Adminmain.Model
{
    using Core.Configuration;
    using Core.Db;
    using WebCoreModule;

    public class AdminMainModelInfoPage
    {
        public AdminMainModelInfoPage() : this(new CoreConfiguration(), new WebCoreConfiguration())
        {
        }

        public AdminMainModelInfoPage(CoreConfiguration appCoreConfiguration, WebCoreConfiguration webCoreConfiguration)
        {
            AppCoreConfiguration = appCoreConfiguration;
            WebCoreConfiguration = webCoreConfiguration;
        }

        public List<Role> Roles { get; set; }

        public List<SelectListItem> ModulesList { get; set; } = new List<SelectListItem>();

        public CoreConfiguration AppCoreConfiguration { get; }

        public WebCoreConfiguration WebCoreConfiguration { get; }
    }
}

