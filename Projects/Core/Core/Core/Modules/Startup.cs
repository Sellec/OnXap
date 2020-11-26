namespace OnXap.Core.Modules
{
    class Startup : StartupBase
    {
        protected override void ConfigureBindings(BindingsCollection bindingsCollection)
        {
            bindingsCollection.SetSingleton<ModuleControllerTypesManager>();
        }
    }
}
