using System;

namespace OnXap.Modules.Auth
{
    using Core.Configuration;
    using Core.Modules;
    using Model;

    public class ModuleAuthControllerAdmin : ModuleControllerAdmin<ModuleAuth, Design.Model.ModuleSettings, Configuration>
    {
        protected override void ConfigurationViewFill(Design.Model.ModuleSettings viewModelForFill, out string viewName)
        {
            using (var db = Module.CreateUnitOfWork())
            {
                viewModelForFill.ApplyConfiguration(Module.GetConfiguration<ModuleConfiguration>());
                viewName = "ModuleSettings.cshtml";
            }
        }

        protected override ModuleConfiguration<ModuleAuth> ConfigurationSaveCustom(Configuration formData, out string outputMessage)
        {
            var cfg = Module.GetConfigurationManipulator().GetEditable<ModuleConfiguration>();

            outputMessage = null;
            return cfg;
        }
    }
}