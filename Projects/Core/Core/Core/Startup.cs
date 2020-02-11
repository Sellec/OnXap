using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Core
{
    class Startup : IConfigureBindings
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<Journaling.JournalingManager>();
            bindingsCollection.SetSingleton<ServiceMonitor.Monitor>();
            bindingsCollection.SetSingleton<Users.IEntitiesManager, Users.EntitiesManager>();
            bindingsCollection.SetSingleton<Users.UsersManager>();

            bindingsCollection.SetSingleton<DbSchema.DbSchemaManagerConfigure>();
            bindingsCollection.SetSingleton<DbSchema.DbSchemaManager>();

            bindingsCollection.SetTransient<DbSchema.DbSchemaDefaultMigration>();
            bindingsCollection.SetTransient<DbSchema.DbSchemaDefaultProfile>();

            bindingsCollection.SetTransient<Db.ItemTypeSchemaItem>();
            bindingsCollection.SetTransient<Db.ModuleConfigSchemaItem>();
            bindingsCollection.SetTransient<Db.RolePermissionSchemaItem>();
            bindingsCollection.SetTransient<Db.RoleSchemaItem>();
            bindingsCollection.SetTransient<Db.RoleUserSchemaItem>();
            bindingsCollection.SetTransient<Db.UserHistorySchemaItem>();
            bindingsCollection.SetTransient<Db.UserSchemaItem>();

            bindingsCollection.SetTransient<Items.Db.ItemLinkSchemaItem>();
            bindingsCollection.SetTransient<Items.Db.ItemLinkSchemaItemPre>();
            bindingsCollection.SetTransient<Items.Db.ItemParentSchemaItem>();
        }
    }
}
