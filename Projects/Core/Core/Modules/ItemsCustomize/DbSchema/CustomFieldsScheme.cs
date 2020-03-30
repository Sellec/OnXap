using FluentMigrator.SqlServer;

namespace OnXap.Modules.ItemsCustomize.DbSchema
{
    using Core.DbSchema;

    class CustomFieldsScheme : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            var isTableExists = Schema.Table<DB.CustomFieldsScheme>().Exists();
            var tableName = GetTableName<DB.CustomFieldsScheme>();

            if (!isTableExists)
            {
                Create.Table<DB.CustomFieldsScheme>().
                    WithColumn((DB.CustomFieldsScheme x) => x.IdScheme).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((DB.CustomFieldsScheme x) => x.IdModule).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((DB.CustomFieldsScheme x) => x.NameScheme).AsString(200).NotNullable().
                    WithColumn((DB.CustomFieldsScheme x) => x.UniqueKey).AsString(500).Nullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (DB.CustomFieldsScheme x) => x.IdScheme, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsScheme x) => x.IdModule, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (DB.CustomFieldsScheme x) => x.NameScheme, x => x.AsString(200).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (DB.CustomFieldsScheme x) => x.UniqueKey, x => x.AsString(500).Nullable());
            }

            if (!Schema.Table<DB.CustomFieldsScheme>().Exists() || !Schema.Table<DB.CustomFieldsScheme>().Index($"{tableName}_UniqueKey").Exists())
                if (!Schema.Table<DB.CustomFieldsScheme>().Index($"UniqueKey").Exists())
                    Create.Index($"{tableName}_UniqueKey").OnTable(GetTableName<DB.CustomFieldsScheme>()).
                        OnColumn(GetColumnName((DB.CustomFieldsScheme x) => x.UniqueKey)).Ascending().
                        WithOptions().UniqueNullsNotDistinct();
        }
    }
}
