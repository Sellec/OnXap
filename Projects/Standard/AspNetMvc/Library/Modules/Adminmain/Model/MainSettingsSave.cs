namespace OnXap.Modules.Adminmain.Model
{
    using Core.Configuration;
    using WebCoreModule;

    public class MainSettingsSave
    {
        public MainSettingsSave() : this(new CoreConfiguration(), new WebCoreConfiguration())
        {
        }

        public MainSettingsSave(CoreConfiguration appCoreConfiguration, WebCoreConfiguration webCoreConfiguration)
        {
            AppCoreConfiguration = appCoreConfiguration;
            WebCoreConfiguration = webCoreConfiguration;
        }

        public CoreConfiguration AppCoreConfiguration { get; set; }
        public WebCoreConfiguration WebCoreConfiguration { get; set; }
    }
}

