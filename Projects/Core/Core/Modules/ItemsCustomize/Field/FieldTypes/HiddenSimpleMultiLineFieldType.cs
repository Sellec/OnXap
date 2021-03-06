﻿using System.Collections.Generic;
using System.Linq;

namespace OnXap.Modules.ItemsCustomize.Field.FieldTypes
{
#pragma warning disable CS1591 // todo внести комментарии.
    public class HiddenSimpleMultiLineFieldType : FieldType
    {
        public override ValuesValidationResult Validate(IEnumerable<object> values, IField field)
        {
            if (values.Count() > 0 && values.First() != null && !values.First().GetType().IsValueType)
            {
                var validationValue = values.First().ToString();
            }

            return new ValuesValidationResult(values);
        }

        public override int IdType
        {
            get => 9;
        }

        public override string TypeName
        {
            get => "Скрытое многострочное поле";
        }

        public override bool IsRawOrSourceValue
        {
            get => true;
        }

        public override bool? ForcedIsMultipleValues
        {
            get => false;
        }
    }
}
