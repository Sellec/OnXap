using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;
using System.Linq;
using System.Web.Mvc;

namespace OnXap.Modules.ItemsCustomize
{
    using Modules.ItemsCustomize.Field.FieldTypes;
    using Modules.ItemsCustomize.Field.FieldTypes;

    class Startup : IConfigureBindings, IExecuteStart
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetTransient<ICustomFieldRender<HiddenSimpleMultiLineFieldType>, HiddenSimpleMultiLineFieldTypeRender>();
            bindingsCollection.SetTransient<ICustomFieldRender<HiddenSingleLineFieldType>, HiddenSingleLineFieldTypeRender>();
            bindingsCollection.SetTransient<ICustomFieldRender<SimpleMultiLineFieldType>, SimpleMultiLineFieldTypeRender>();
            bindingsCollection.SetTransient<ICustomFieldRender<SimpleSingleLineFieldType>, SimpleSingleLineFieldTypeRender>();
            bindingsCollection.SetTransient<ICustomFieldRender<SourceMultipleFieldType>, SourceMultipleFieldTypeRender>();
            bindingsCollection.SetTransient<ICustomFieldRender<SourceSingleFieldType>, SourceSingleFieldTypeRender>();
            bindingsCollection.SetTransient<ICustomFieldRender<UnknownFieldType>, UnknownFieldTypeRender>();
        }

        void IExecuteStart<OnXApplication>.ExecuteStart(OnXApplication core)
        {
            if (!ValueProviderFactories.Factories.Any(x => x is MetadataAndValues.FieldValueProviderFactory))
            {
                ValueProviderFactories.Factories.Add(new MetadataAndValues.FieldValueProviderFactory());
            }
        }
    }
}