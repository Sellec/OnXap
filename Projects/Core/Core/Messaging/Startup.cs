using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Messaging
{
    using Subscriptions;

    class Startup : IConfigureBindings
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<ISubscriptionsManager, SubscriptionsManager>();
            bindingsCollection.SetTransient<DB.MessageSubscriptionRoleSchemaItem>();
            bindingsCollection.SetTransient<DB.MessageSubscriptionSchemaItem>();
        }
    }
}

