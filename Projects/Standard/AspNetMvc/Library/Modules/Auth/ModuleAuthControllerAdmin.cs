using System;

namespace OnXap.Modules.Auth
{
    using Core.Configuration;
    using Core.Modules;
    using Model;

    public class ModuleAuthControllerAdmin : ModuleControllerAdmin<ModuleAuth, ViewModels.ModuleSettings, Configuration>
    {
        protected override void ConfigurationViewFill(ViewModels.ModuleSettings viewModelForFill, out string viewName)
        {
            viewModelForFill.ApplyConfiguration(Module.GetConfiguration<ModuleConfiguration>());
            viewName = "ModuleSettings.cshtml";
        }

        protected override ModuleConfiguration<ModuleAuth> ConfigurationSaveCustom(Configuration formData, out string outputMessage)
        {
            var cfg = Module.GetConfigurationManipulator().GetEditable<ModuleConfiguration>();

            outputMessage = null;
            return cfg;
        }
    }
}