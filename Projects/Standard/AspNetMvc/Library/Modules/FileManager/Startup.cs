using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Modules.FileManager
{
    using Core.Modules;
    using CustomFieldsFileTypes;
    using Modules.ItemsCustomize;

    class Startup : IConfigureBindings
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<FileManager>();
            bindingsCollection.SetTransient<IModuleController<FileManager>, FileManagerController>();

            bindingsCollection.SetTransient<ICustomFieldRender<FileImageFieldType>, FileImageFieldTypeRender>();
            bindingsCollection.SetTransient<ICustomFieldRender<FileFieldType>, FileFieldTypeRender>();

            bindingsCollection.SetTransient<DbSchema.File>();
            bindingsCollection.SetTransient<DbSchema.FileRemoveQueue>();
            bindingsCollection.SetTransient<DbSchema.File20200306>();
        }
    }
}
