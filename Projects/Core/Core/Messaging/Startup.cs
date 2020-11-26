namespace OnXap.Messaging
{
    using Subscriptions;

    class Startup : StartupBase
    {
        protected override void ConfigureBindings(BindingsCollection bindingsCollection)
        {
            bindingsCollection.SetSingleton<ISubscriptionsManager, SubscriptionsManager>();
            bindingsCollection.SetTransient<DbSchema.MessageSubscriptionRole>();
            bindingsCollection.SetTransient<DbSchema.MessageSubscription>();
            bindingsCollection.SetTransient<DbSchema.MessageQueue>();
            bindingsCollection.SetTransient<DbSchema.MessageQueueHistory>();
        }
    }
}

