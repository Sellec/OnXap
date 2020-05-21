using System;

namespace OnXap.Modules.FileManager
{
    using Core.Configuration;
    using Core.Modules;
    using Model;

    public class FileManagerControllerAdmin : ModuleControllerAdmin<FileManager, ViewModels.ModuleSettings, Configuration>
    {
        protected override void ConfigurationViewFill(ViewModels.ModuleSettings viewModelForFill, out string viewName)
        {
            viewModelForFill.ApplyConfiguration(Module.GetConfiguration<FileManagerConfiguration>());
            viewName = "ModuleSettings.cshtml";
        }

        protected override ModuleConfiguration<FileManager> ConfigurationSaveCustom(Configuration formData, out string outputMessage)
        {
            var cfg = Module.GetConfigurationManipulator().GetEditable<FileManagerConfiguration>();
            cfg.IsCheckRemovedFiles = formData.IsCheckRemovedFiles;
            Module.GetConfigurationManipulator().ApplyConfiguration(cfg);

            outputMessage = null;
            return cfg;
        }
    }
}