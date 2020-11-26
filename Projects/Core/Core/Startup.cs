namespace OnXap
{
    class Startup : StartupBase
    {
        protected override void ConfigureBindings(BindingsCollection bindingsCollection)
        {
            bindingsCollection.SetSingleton<Modules.CoreModule.CoreModule>();
            bindingsCollection.SetSingleton<Modules.UsersManagement.ModuleUsersManagement>();
            bindingsCollection.SetSingleton<TaskSheduling.TaskSchedulingManager>();
            bindingsCollection.SetTransient<TaskSheduling.Db.TaskSchema>();
            bindingsCollection.SetTransient<TaskSheduling.Db.TaskScheduleSchema>();
        }
    }
}
