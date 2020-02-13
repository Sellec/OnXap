using FluentMigrator.SqlServer;

namespace OnXap.Modules.ItemsCustomize.DbSchema
{
    using Core.DbSchema;

    class CustomFieldsSchemeData : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            var isTableExists = Schema.Table<DB.CustomFieldsSchemeData>().Exists();

            if (!isTableExists)
            {
                Create.Table<DB.CustomFieldsSchemeData>().
                    WithColumn((DB.CustomFieldsSchemeData x) => x.IdModule).AsInt32().NotNullable().
                    WithColumn((DB.CustomFieldsSchemeData x) => x.IdItemType).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((DB.CustomFieldsSchemeData x) => x.IdScheme).AsInt32().NotNullable().
                    WithColumn((DB.CustomFieldsSchemeData x) => x.IdSchemeItem).AsInt32().NotNullable().
                    WithColumn((DB.CustomFieldsSchemeData x) => x.IdField).AsInt32().NotNullable().
                    WithColumn((DB.CustomFieldsSchemeData x) => x.Order).AsInt32().NotNullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (DB.CustomFieldsSchemeData x) => x.IdModule, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsSchemeData x) => x.IdItemType, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (DB.CustomFieldsSchemeData x) => x.IdScheme, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsSchemeData x) => x.IdSchemeItem, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsSchemeData x) => x.IdField, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsSchemeData x) => x.Order, x => x.AsInt32().NotNullable());
            }

            if (!Schema.Table<DB.CustomFieldsSchemeData>().Exists() || !Schema.Table<DB.CustomFieldsSchemeData>().Index("PK_CustomFieldsSchemeData").Exists())
                Create.PrimaryKey("PK_CustomFieldsSchemeData").OnTable(GetTableName<DB.CustomFieldsSchemeData>()).Columns(
                    GetColumnName((DB.CustomFieldsSchemeData x) => x.IdModule),
                    GetColumnName((DB.CustomFieldsSchemeData x) => x.IdItemType),
                    GetColumnName((DB.CustomFieldsSchemeData x) => x.IdScheme),
                    GetColumnName((DB.CustomFieldsSchemeData x) => x.IdSchemeItem),
                    GetColumnName((DB.CustomFieldsSchemeData x) => x.IdField)
                );
        }
    }
}
