using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace OnXap.Modules.ItemsCustomize.MetadataAndValues
{
    using Modules.ItemsCustomize.Field;

    class FieldValueProviderResult : ValueProviderResult
    {
        private IField _field = null;

        public FieldValueProviderResult(IField field, string[] rawValue, CultureInfo culture) : base(rawValue, rawValue?.FirstOrDefault(), culture)
        {
            RawFromForm = rawValue;
            _field = field;
        }

        public string[] RawFromForm { get; private set; }
    }
}
