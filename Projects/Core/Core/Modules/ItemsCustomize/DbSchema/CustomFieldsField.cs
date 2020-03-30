using FluentMigrator.SqlServer;

namespace OnXap.Modules.ItemsCustomize.DbSchema
{
    using Core.DbSchema;

    class CustomFieldsField : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            var isTableExists = Schema.Table<DB.CustomFieldsField>().Exists();
            var tableName = GetTableName<DB.CustomFieldsField>();

            if (!isTableExists)
            {
                Create.Table<DB.CustomFieldsField>().
                    WithColumn((DB.CustomFieldsField x) => x.IdField).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((DB.CustomFieldsField x) => x.IdModule).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((DB.CustomFieldsField x) => x.name).AsString(200).NotNullable().WithDefaultValue("").
                    WithColumn((DB.CustomFieldsField x) => x.nameAlt).AsString(200).Nullable().
                    WithColumn((DB.CustomFieldsField x) => x.IdFieldType).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((DB.CustomFieldsField x) => x.alias).AsString(100).Nullable().
                    WithColumn((DB.CustomFieldsField x) => x.IdValueType).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((DB.CustomFieldsField x) => x.size).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((DB.CustomFieldsField x) => x.field_data).AsString(int.MaxValue).Nullable().
                    WithColumn((DB.CustomFieldsField x) => x.IsValueRequired).AsBoolean().NotNullable().WithDefaultValue(false).
                    WithColumn((DB.CustomFieldsField x) => x.IsMultipleValues).AsBoolean().NotNullable().WithDefaultValue(false).
                    WithColumn((DB.CustomFieldsField x) => x.status).AsInt32().NotNullable().WithDefaultValue(1).
                    WithColumn((DB.CustomFieldsField x) => x.Block).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((DB.CustomFieldsField x) => x.nameEnding).AsString(100).Nullable().WithDefaultValue(null).
                    WithColumn((DB.CustomFieldsField x) => x.formatCheck).AsString(200).Nullable().WithDefaultValue(null).
                    WithColumn((DB.CustomFieldsField x) => x.IdSource).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((DB.CustomFieldsField x) => x.ParameterNumeric01).AsFloat().NotNullable().WithDefaultValue(0).
                    WithColumn((DB.CustomFieldsField x) => x.ParameterNumeric02).AsFloat().NotNullable().WithDefaultValue(0).
                    WithColumn((DB.CustomFieldsField x) => x.DateChange).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((DB.CustomFieldsField x) => x.UniqueKey).AsString(400).Nullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.IdField, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.IdModule, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.name, x => x.AsString(200).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.nameAlt, x => x.AsString(200).Nullable());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.IdFieldType, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.alias, x => x.AsString(100).Nullable());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.IdValueType, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.size, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.field_data, x => x.AsString(int.MaxValue).Nullable());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.IsValueRequired, x => x.AsBoolean().NotNullable().WithDefaultValue(false));
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.IsMultipleValues, x => x.AsBoolean().NotNullable().WithDefaultValue(false));
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.status, x => x.AsInt32().NotNullable().WithDefaultValue(1));
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.Block, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.nameEnding, x => x.AsString(100).Nullable().WithDefaultValue(null));
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.formatCheck, x => x.AsString(200).Nullable().WithDefaultValue(null));
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.IdSource, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.ParameterNumeric01, x => x.AsFloat().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.ParameterNumeric02, x => x.AsFloat().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.DateChange, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (DB.CustomFieldsField x) => x.UniqueKey, x => x.AsString(400).Nullable());
            }

            if (!Schema.Table<DB.CustomFieldsField>().Exists() || !Schema.Table<DB.CustomFieldsField>().Index($"{tableName}_UniqueKey").Exists())
                if (!Schema.Table<DB.CustomFieldsField>().Index($"UniqueKey").Exists())
                    Create.Index($"{tableName}_UniqueKey").OnTable(GetTableName<DB.CustomFieldsField>()).
                        OnColumn(GetColumnName((DB.CustomFieldsField x) => x.UniqueKey)).Ascending().
                        WithOptions().UniqueNullsNotDistinct();

            if (!isTableExists || !Schema.Table<DB.CustomFieldsField>().Index($"{tableName}_field_id").Exists())
                if (!Schema.Table<DB.CustomFieldsField>().Index($"field_id").Exists())
                    Create.Index($"{tableName}_field_id").OnTable(GetTableName<DB.CustomFieldsField>()).
                        OnColumn(GetColumnName((DB.CustomFieldsField x) => x.IdField)).Ascending().
                        OnColumn(GetColumnName((DB.CustomFieldsField x) => x.IdModule)).Ascending();

            if (!isTableExists || !Schema.Table<DB.CustomFieldsField>().Index($"{tableName}_field_module1").Exists())
                if (!Schema.Table<DB.CustomFieldsField>().Index($"field_module1").Exists())
                    Create.Index($"{tableName}_field_module1").OnTable(GetTableName<DB.CustomFieldsField>()).
                        OnColumn(GetColumnName((DB.CustomFieldsField x) => x.IdModule)).Ascending();

            if (!isTableExists || !Schema.Table<DB.CustomFieldsField>().Index($"{tableName}_field_module1_2").Exists())
                if (!Schema.Table<DB.CustomFieldsField>().Index($"field_module1_2").Exists())
                    Create.Index($"{tableName}_field_module1_2").OnTable(GetTableName<DB.CustomFieldsField>()).
                        OnColumn(GetColumnName((DB.CustomFieldsField x) => x.IdModule)).Ascending().
                        OnColumn(GetColumnName((DB.CustomFieldsField x) => x.name)).Ascending();
        }
    }
}
