using Newtonsoft.Json;
using System.Linq;

namespace OnXap.Modules.MessagingEmail
{
    using Core.Configuration;
    using Messaging;
    using Messaging.Components;
    using Model;
    using Modules.CoreModule;

    class EMailController : Core.Modules.ModuleControllerAdmin<EMailModule, Configuration, Configuration>
    {
        protected override void ConfigurationViewFill(Configuration viewModelForFill, out string viewName)
        {
            viewName = "ModuleSettings.cshtml";

            var handlers = AppCore.AppConfig.MessagingServicesComponentsSettings.
                Where(x => x.TypeFullName.StartsWith(typeof(Components.SmtpServer).Namespace)).
                Select(x => new { x.TypeFullName, Settings = JsonConvert.DeserializeObject<Components.SmtpServerSettings>(x.SettingsSerialized) }).
                ToList();

            var smtp = handlers.Where(x => x.TypeFullName.EndsWith("." + nameof(Components.SmtpServer))).Select(x => x.Settings).FirstOrDefault();

            viewModelForFill.ApplyConfiguration(smtp);
        }

        protected override ModuleConfiguration<EMailModule> ConfigurationSaveCustom(Configuration formData, out string outputMessage)
        {
            var handlers = AppCore.AppConfig.MessagingServicesComponentsSettings.ToDictionary(x => x.TypeFullName, x => x);

            handlers.Remove(typeof(Components.SmtpServer).FullName);
            if (formData.IsUseSmtp)
            {
                handlers[typeof(Components.SmtpServer).FullName] = new ComponentSettings()
                {
                    TypeFullName = typeof(Components.SmtpServer).FullName,
                    SettingsSerialized = JsonConvert.SerializeObject(new Components.SmtpServerSettings()
                    {
                        Server = formData?.Smtp?.Server,
                        IsSecure = formData?.Smtp?.IsSecure ?? false,
                        Port = formData?.Smtp?.Port,
                        Login = formData?.Smtp?.Login,
                        Password = formData?.Smtp?.Password,
                        IsIgnoreCertificateErrors = formData?.Smtp?.IsIgnoreCertificateErrors ?? false
                    })
                };
            }

            var cfg = AppCore.Get<CoreModule>().GetConfigurationManipulator().GetEditable<CoreConfiguration>();

            cfg.MessagingServicesComponentsSettings = handlers.Values.ToList();

            AppCore.Get<CoreModule>().GetConfigurationManipulator().ApplyConfiguration(cfg);
            AppCore.Get<MessagingManager>().UpdateComponentsFromSettings();

            return base.ConfigurationSaveCustom(formData, out outputMessage);
        }
    }
}
