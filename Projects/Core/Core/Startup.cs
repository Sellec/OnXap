using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap
{
    class Startup : IConfigureBindings
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<Modules.CoreModule.CoreModule>();
            bindingsCollection.SetSingleton<Modules.UsersManagement.ModuleUsersManagement>();
        }
    }
}
