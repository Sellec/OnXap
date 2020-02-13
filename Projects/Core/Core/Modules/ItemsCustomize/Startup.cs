using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Modules.ItemsCustomize
{
    class Startup : IConfigureBindings
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
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
