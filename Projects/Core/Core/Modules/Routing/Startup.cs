namespace OnXap.Modules.Routing
{
    class Startup : StartupBase
    {
        protected override void ConfigureBindings(BindingsCollection bindingsCollection)
        {
            bindingsCollection.SetSingleton<ModuleRouting>();
            bindingsCollection.SetSingleton<UrlManager>();
            bindingsCollection.SetTransient<DbSchema.Routing>();
        }
    }
}
