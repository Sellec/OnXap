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
            bindingsCollection.SetTransient<DbSchema.MessageSubscriptionRole>();
            bindingsCollection.SetTransient<DbSchema.MessageSubscription>();
            bindingsCollection.SetTransient<DbSchema.MessageQueue>();
            bindingsCollection.SetTransient<DbSchema.MessageQueueHistory>();
        }
    }
}

