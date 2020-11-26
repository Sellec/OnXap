namespace OnXap.Modules.UsersManagement
{
    class Startup : StartupBase
    {
        protected override void ConfigureBindings(BindingsCollection bindingsCollection)
        {
            bindingsCollection.SetSingleton<ModuleUsersManagement>();
        }
    }
}
