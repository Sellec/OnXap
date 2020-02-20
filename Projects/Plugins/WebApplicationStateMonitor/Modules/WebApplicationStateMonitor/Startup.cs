using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Modules.WebApplicationStateMonitor
{
    class Startup : IConfigureBindings, IConfigureBindingsLazy
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<WebApplicationStateMonitor>();
        }

        void IConfigureBindingsLazy<OnXApplication>.ConfigureBindingsLazy(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<WebApplicationStateMonitor>();
        }
    }
}
