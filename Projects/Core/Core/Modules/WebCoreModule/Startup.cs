namespace OnXap.Modules.WebCoreModule
{
    class Startup : StartupBase
    {
        protected override void ConfigureBindings(BindingsCollection bindingsCollection)
        {
            bindingsCollection.RegisterBindingConstraintHandler(new BindingConstraint());
            bindingsCollection.SetSingleton<WebCoreModule>();
            bindingsCollection.SetSingleton<WebCoreConfigurationChecker>();
        }
    }
}
