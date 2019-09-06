using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Messaging
{
    class Startup : IConfigureBindings
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<ISubscriptionsManager, SubscriptionsManager>();
        }
    }
}

