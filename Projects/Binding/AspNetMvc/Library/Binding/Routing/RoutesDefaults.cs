using System.Web.Routing;

namespace OnXap.Binding.Routing
{
    using Core.Modules;
    using Languages;

    public class RouteWithDefaults : Route
    {
        class RoutesDefaults
        {
            public RoutesDefaults(OnXApplication core)
            {
                var moduleID = core.WebConfig.IdModuleDefault;
                var module = core.GetModulesManager().GetModule(moduleID) ?? core.GetModulesManager().GetModule<Modules.Default.ModuleDefault>();

                Controller = module.IdModule.ToString();
            }

            public string Controller
            {
                get;
            }

            public string Action
            {
                get => nameof(ModuleControllerUser<Modules.WebCoreModule.WebCoreModule>.Index);
            }

            public System.Web.Mvc.UrlParameter Url
            {
                get => System.Web.Mvc.UrlParameter.Optional;
            }
        }

        class RoutesDefaultsWithLanguage : RoutesDefaults
        {
            public RoutesDefaultsWithLanguage(OnXApplication core) : base(core)
            {
                Language = core.Get<Manager>().GetUserLanguage().ShortName;
            }

            public string Language
            {
                get;
            }
        }

        private readonly OnXApplication _core = null;
        private readonly bool _defaultsWithLanguage;

        public RouteWithDefaults(OnXApplication core, string url, bool defaultsWithLanguage, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler)
            : base(url, new RouteValueDictionary(defaultsWithLanguage ? new RoutesDefaultsWithLanguage(core) : new RoutesDefaults(core)), constraints, dataTokens, routeHandler)
        {
            _core = core;
            _defaultsWithLanguage = defaultsWithLanguage;
        }

        public void UpdateDefaults()
        {
            Defaults = new RouteValueDictionary(_defaultsWithLanguage ? new RoutesDefaultsWithLanguage(_core) : new RoutesDefaults(_core));
        }
    }
}
