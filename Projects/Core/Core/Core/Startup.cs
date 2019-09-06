using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Core
{
    class Startup : IConfigureBindings
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<Journaling.JournalingManager>();
            bindingsCollection.SetSingleton<ServiceMonitor.Monitor>();
            bindingsCollection.SetSingleton<Users.IEntitiesManager, Users.EntitiesManager>();
            bindingsCollection.SetSingleton<Users.UsersManager>();
        }
    }
}
