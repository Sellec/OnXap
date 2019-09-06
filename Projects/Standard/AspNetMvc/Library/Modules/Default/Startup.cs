using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Modules.Default
{
    using Core.Modules;

    class Startup : IConfigureBindings
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetTransient<IModuleController<ModuleDefault>, ModuleDefaultController>();
        }
    }
}