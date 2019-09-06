using System.Web.Mvc;

namespace OnXap.Modules.ItemsCustomize.MetadataAndValues
{
    class FieldValueProviderFactory : ValueProviderFactory
    {
        public override IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            return new FieldValueProvider(controllerContext);
        }
    }
}
