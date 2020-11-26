namespace OnXap.Modules.ItemsCustomize
{
    class Startup : StartupBase
    {
        protected override void ConfigureBindings(BindingsCollection bindingsCollection)
        {
            bindingsCollection.SetSingleton<ModuleItemsCustomize>();
            bindingsCollection.SetTransient<DbSchema.CustomFieldsField>();
            bindingsCollection.SetTransient<DbSchema.CustomFieldsData>();
            bindingsCollection.SetTransient<DbSchema.CustomFieldsScheme>();
            bindingsCollection.SetTransient<DbSchema.CustomFieldsSchemeData>();
            bindingsCollection.SetTransient<DbSchema.CustomFieldsValue>();
        }
    }
}
