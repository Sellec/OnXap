using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Modules.Auth
{
    using Core.Modules;

    class Startup : IConfigureBindings
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetTransient<IModuleController<ModuleAuth>>(typeof(ModuleAuthController), typeof(ModuleAuthControllerAdmin));
        }
    }
}
