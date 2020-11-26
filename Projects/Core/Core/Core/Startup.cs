namespace OnXap.Core
{
    class Startup : StartupBase
    {
        protected override void ConfigureBindings(BindingsCollection bindingsCollection)
        {
            bindingsCollection.SetSingleton<Journaling.JournalingManager>();
            bindingsCollection.SetSingleton<ServiceMonitor.Monitor>();
            bindingsCollection.SetSingleton<Users.UsersManager>();

            bindingsCollection.SetSingleton<DbSchema.DbSchemaManagerConfigure>();
            bindingsCollection.SetSingleton<DbSchema.DbSchemaManager>();

            bindingsCollection.SetTransient<DbSchema.DbSchemaDefaultMigration>();
            bindingsCollection.SetTransient<DbSchema.DbSchemaDefaultProfile>();

            bindingsCollection.SetTransient<Db.InsertOnDuplicateUpdateSchemaItem>();

            bindingsCollection.SetTransient<Db.ItemTypeSchemaItem>();
            bindingsCollection.SetTransient<Db.ModuleConfigSchemaItem>();
            bindingsCollection.SetTransient<Db.RolePermissionSchemaItem>();
            bindingsCollection.SetTransient<Db.RoleSchemaItem>();
            bindingsCollection.SetTransient<Db.RoleUserSchemaItem>();
            bindingsCollection.SetTransient<Db.UserHistorySchemaItem>();
            bindingsCollection.SetTransient<Db.UserSchemaItem>();
            bindingsCollection.SetTransient<Db.UserSchemaItem202003100112>();

            bindingsCollection.SetTransient<Items.Db.ItemLinkSchemaItem>();
            bindingsCollection.SetTransient<Items.Db.ItemParentSchemaItem>();
        }
    }
}
