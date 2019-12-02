using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Modules.ItemsCustomize
{
    using Core.Modules;

    class Startup : IConfigureBindings
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<ModuleItemsCustomize2>();
            bindingsCollection.SetTransient<IModuleController<ModuleItemsCustomize2>, ModuleController>();
        }
    }
}