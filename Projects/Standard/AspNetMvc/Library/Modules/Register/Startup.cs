using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Modules.Register
{
    using Core.Modules;

    class Startup : IConfigureBindings
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<ModuleRegister>();
            bindingsCollection.SetTransient<IModuleController<ModuleRegister>, ModuleRegisterController>();
        }
    }
}
