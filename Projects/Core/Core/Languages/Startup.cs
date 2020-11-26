namespace OnXap.Languages
{
    class Startup : StartupBase
    {
        protected override void ConfigureBindings(BindingsCollection bindingsCollection)
        {
            bindingsCollection.SetTransient<DB.LanguageSchemaItem>();
        }
    }
}
