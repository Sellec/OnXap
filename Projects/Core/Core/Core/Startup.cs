using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Core
{
    class Startup : IConfigureBindings
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<DbSchema.DbSchemaManager>();
            bindingsCollection.SetSingleton<Journaling.JournalingManager>();
            bindingsCollection.SetSingleton<ServiceMonitor.Monitor>();
            bindingsCollection.SetSingleton<Users.IEntitiesManager, Users.EntitiesManager>();
            bindingsCollection.SetSingleton<Users.UsersManager>();

            bindingsCollection.SetTransient<DbSchema.DbSchemaDefaultMigration>();
            bindingsCollection.SetTransient<DbSchema.DbSchemaDefaultProfile>();
            bindingsCollection.SetTransient<Db.ModuleConfigSchemaItem>();
        }
    }
}
