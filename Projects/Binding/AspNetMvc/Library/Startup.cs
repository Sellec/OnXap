using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Binding
{
    using Binding.Providers;
    using Modules.Auth;

    class Startup : IConfigureBindings
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<ModuleAuth, Module2>();
            bindingsCollection.SetSingleton<SessionBinder>();
            bindingsCollection.SetTransient<Core.Db.UserSessionSchemaItem>();
        }
    }
}
