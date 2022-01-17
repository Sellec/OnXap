namespace OnXap.Modules.Subscriptions
{
    class Startup : StartupBase
    {
        protected override void ConfigureBindings(BindingsCollection bindingsCollection)
        {
            bindingsCollection.SetSingleton<SubscriptionsManager>();
            bindingsCollection.SetTransient<DbSchema.Subscription>();
            bindingsCollection.SetTransient<DbSchema.SubscriptionGroup>();
            bindingsCollection.SetTransient<DbSchema.SubscriptionMessagingContact>();
            bindingsCollection.SetTransient<DbSchema.SubscriptionUser>();
        }

        protected override void ExecuteStart(OnXApplication core)
        {
        }
    }
}