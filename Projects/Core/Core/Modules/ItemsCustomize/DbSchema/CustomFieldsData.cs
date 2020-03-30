using FluentMigrator.SqlServer;
using System;

namespace OnXap.Modules.ItemsCustomize.DbSchema
{
    using Core.DbSchema;

    class CustomFieldsData : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            var isTableExists = Schema.Table<DB.CustomFieldsData>().Exists();
            var tableName = GetTableName<DB.CustomFieldsData>();

            if (!isTableExists)
            {
                Create.Table<DB.CustomFieldsData>().
                    WithColumn((DB.CustomFieldsData x) => x.IdFieldData).AsInt64().NotNullable().PrimaryKey().Identity().
                    WithColumn((DB.CustomFieldsData x) => x.IdField).AsInt32().NotNullable().
                    WithColumn((DB.CustomFieldsData x) => x.IdItem).AsInt32().NotNullable().
                    WithColumn((DB.CustomFieldsData x) => x.IdItemType).AsInt32().NotNullable().
                    WithColumn((DB.CustomFieldsData x) => x.IdFieldValue).AsInt32().NotNullable().
                    WithColumn((DB.CustomFieldsData x) => x.FieldValue).AsString(int.MaxValue).NotNullable().
                    WithColumn((DB.CustomFieldsData x) => x.DateChange).AsInt32().NotNullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (DB.CustomFieldsData x) => x.IdFieldData, x => x.AsInt64().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsData x) => x.IdField, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsData x) => x.IdItem, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsData x) => x.IdItemType, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsData x) => x.IdFieldValue, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsData x) => x.FieldValue, x => x.AsString(int.MaxValue).NotNullable());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsData x) => x.DateChange, x => x.AsInt32().NotNullable());
            }

            if (!isTableExists || !Schema.Table<DB.CustomFieldsData>().Index($"{tableName}_IdField_IdItem_IdItemType_with_FieldValue").Exists())
                if (!Schema.Table<DB.CustomFieldsData>().Index($"IdField_IdItem_IdItemType_with_FieldValue").Exists())
                    Create.Index("IdField_IdItem_IdItemType_with_FieldValue").OnTable(GetTableName<DB.CustomFieldsData>()).
                        OnColumn(GetColumnName((DB.CustomFieldsData x) => x.IdField)).Ascending().
                        OnColumn(GetColumnName((DB.CustomFieldsData x) => x.IdItem)).Ascending().
                        OnColumn(GetColumnName((DB.CustomFieldsData x) => x.IdItemType)).Ascending().
                        Include(GetColumnName((DB.CustomFieldsData x) => x.FieldValue));

            if (!isTableExists || !Schema.Table<DB.CustomFieldsData>().Index($"{tableName}_IdField_IdItem_with_FieldValue").Exists())
                if (!Schema.Table<DB.CustomFieldsData>().Index($"IdField_IdItem_with_FieldValue").Exists())
                    Create.Index($"{tableName}_IdField_IdItem_with_FieldValue").OnTable(GetTableName<DB.CustomFieldsData>()).
                        OnColumn(GetColumnName((DB.CustomFieldsData x) => x.IdField)).Ascending().
                        OnColumn(GetColumnName((DB.CustomFieldsData x) => x.IdItem)).Ascending().
                        Include(GetColumnName((DB.CustomFieldsData x) => x.FieldValue));

            if (!isTableExists || !Schema.Table<DB.CustomFieldsData>().Index($"{tableName}_IdField_with_IdItem_IdFieldValue").Exists())
                if (!Schema.Table<DB.CustomFieldsData>().Index($"IdField_with_IdItem_IdFieldValue").Exists())
                    Create.Index($"{tableName}_IdField_with_IdItem_IdFieldValue").OnTable(GetTableName<DB.CustomFieldsData>()).
                        OnColumn(GetColumnName((DB.CustomFieldsData x) => x.IdField)).Ascending().
                        Include(GetColumnName((DB.CustomFieldsData x) => x.IdItem)).
                        Include(GetColumnName((DB.CustomFieldsData x) => x.IdFieldValue));

            if (!isTableExists || !Schema.Table<DB.CustomFieldsData>().Index($"{tableName}_IdItem_IdItemType").Exists())
                if (!Schema.Table<DB.CustomFieldsData>().Index($"IdItem_IdItemType").Exists())
                    Create.Index($"{tableName}_IdItem_IdItemType").OnTable(GetTableName<DB.CustomFieldsData>()).
                        OnColumn(GetColumnName((DB.CustomFieldsData x) => x.IdItem)).Ascending().
                        OnColumn(GetColumnName((DB.CustomFieldsData x) => x.IdItemType)).Ascending();

        }
    }
}
