using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Modules.Materials
{
    using Core.Modules;
    using Core.Items;

    class Startup : IConfigureBindings, IExecuteStart
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<ModuleMaterials>();
            bindingsCollection.AddTransient<IModuleController<ModuleMaterials>, ModuleController>();
            bindingsCollection.AddTransient<IModuleController<ModuleMaterials>, ModuleAdminController>();
            bindingsCollection.SetTransient<MaterialsSitemapProvider>();
        }

        void IExecuteStart<OnXApplication>.ExecuteStart(OnXApplication core)
        {
            core.Get<ItemsManager>().RegisterModuleItemType<DB.News, ModuleMaterials>();
            core.Get<ItemsManager>().RegisterModuleItemType<DB.Page, ModuleMaterials>();
        }
    }
}