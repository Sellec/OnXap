﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace OnXap.Modules.FileManager.CustomFieldsFileTypes
{
    using Core.Data;
    using Db;
    using Modules.ItemsCustomize.Field;

    public class FileImageFieldType : FileFieldType
    {
        internal const int IdFieldTypeImage = 12;

        public override ValuesValidationResult Validate(IEnumerable<object> values, IField field)
        {
            if (field.IsValueRequired && (values == null || values.Count() == 0)) return CreateResultForEmptyValue(field);

            var sourceValues = field.data;
            var unknownValues = values.Where(x => sourceValues.Where(y => y.IdFieldValue == (int)x).Count() == 0);

            var valuesPrepared = new HashSet<int>();
            var valuesInvalid = new System.Collections.ObjectModel.Collection<string>();
            foreach (var value in values)
            {
                if (value is int) valuesPrepared.Add((int)value);
                else if (value is File) valuesPrepared.Add((value as File).IdFile);
                else valuesInvalid.Add(value?.ToString()?.Truncate(0, 10, "..."));
            }

            using (var db = new DataContext())
            {
                var filesFound = db.File.In(valuesPrepared, x => x.IdFile).ToList();
                if (field.IsValueRequired && filesFound.Count == 0) return CreateResultForEmptyValue(field);

                var filesFoundIds = filesFound.Select(x => x.IdFile).ToList();
                var filesUnknown = valuesPrepared.Where(x => !filesFoundIds.Contains(x)).ToList();
                var filesTypeMismatch = filesFound.Where(x => x.TypeCommon != FileTypeCommon.Image).Select(x => x.IdFile).ToList();

                if (filesUnknown.Count > 0 || valuesInvalid.Count > 0 || filesTypeMismatch.Count > 0)
                {
                    var errors = new List<string>();
                    if (valuesInvalid.Count > 0) errors.Add("Следующие значения некорректны:\r\n - " + string.Join(";\r\n - ", valuesInvalid) + ".");
                    if (filesUnknown.Count > 0) errors.Add("Следующие файлы не найдены: №" + string.Join(", №", filesUnknown) + ".");
                    if (filesTypeMismatch.Count > 0) errors.Add("Тип файлов не подходит: №" + string.Join(", №", filesTypeMismatch) + ".");

                    return new ValuesValidationResult(string.Join("\r\n", errors));
                }

                return new ValuesValidationResult(filesFound.Select(x => (object)x.IdFile));
            }
        }

        public override int IdType
        {
            get => IdFieldTypeImage;
        }

        public override string TypeName
        {
            get => "Файл-изображение";
        }
    }
}
