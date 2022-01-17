using Newtonsoft.Json;
using System.Linq;

namespace OnXap.Modules.MessagingEmail
{
    using Core.Configuration;
    using Messaging;
    using Messaging.Components;
    using Model;
    using Modules.CoreModule;

    class EMailController : Core.Modules.ModuleControllerAdmin<Module, Configuration, Configuration>
    {
        protected override void ConfigurationViewFill(Configuration viewModelForFill, out string viewName)
        {
            viewName = "ModuleSettings.cshtml";
            viewModelForFill.ApplyConfiguration(Module.GetConfiguration<ModuleConfiguration>());
        }

        protected override ModuleConfiguration<Module> ConfigurationSaveCustom(Configuration formData, out string outputMessage)
        {
            var cfg = Module.GetConfigurationManipulator().GetEditable<ModuleConfiguration>();

            cfg.IsUseSmtp = formData.IsUseSmtp;
            var isAnonymous = formData.IsAnonymous;
            cfg.OutgoingAddress = formData.OutgoingAddress;
            cfg.OutgoingName = formData.OutgoingName;
            cfg.Server = formData.Server;
            cfg.IsSecure = formData.IsSecure;
            cfg.IsIgnoreCertificateErrors = formData.IsIgnoreCertificateErrors;
            cfg.Port = formData.Port;
            cfg.Login = isAnonymous ? null : formData.Login;
            cfg.Password = isAnonymous ? null : formData.Password;

            Module.GetConfigurationManipulator().ApplyConfiguration(cfg);

            return base.ConfigurationSaveCustom(formData, out outputMessage);
        }
    }
}
