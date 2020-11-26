namespace OnXap.Journaling
{
    class Startup : StartupBase
    {
        protected override void ConfigureBindings(BindingsCollection bindingsCollection)
        {
            bindingsCollection.SetTransient<DB.JournalSchemaItem>();
            bindingsCollection.SetTransient<DB.JournalNameSchemaItem>();
        }
    }
}
