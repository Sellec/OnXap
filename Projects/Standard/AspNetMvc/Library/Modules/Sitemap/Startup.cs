using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Modules.Sitemap
{
    using Core.Modules;

    class Startup : IConfigureBindings
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<ModuleSitemap>();
            bindingsCollection.AddTransient<IModuleController<ModuleSitemap>, ModuleController>();
        }
    }
}