namespace OnXap.Modules.ItemsCustomize.DbSchema
{
    using Core.DbSchema;

    class CustomFieldsValue : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            var isTableExists = Schema.Table<DB.CustomFieldsValue>().Exists();

            if (!isTableExists)
            {
                Create.Table<DB.CustomFieldsValue>().
                    WithColumn((DB.CustomFieldsValue x) => x.IdFieldValue).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((DB.CustomFieldsValue x) => x.IdField).AsInt32().NotNullable().
                    WithColumn((DB.CustomFieldsValue x) => x.FieldValue).AsString(200).NotNullable().
                    WithColumn((DB.CustomFieldsValue x) => x.Order).AsInt32().NotNullable().
                    WithColumn((DB.CustomFieldsValue x) => x.DateChange).AsInt32().NotNullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (DB.CustomFieldsValue x) => x.IdFieldValue, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsValue x) => x.IdField, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsValue x) => x.FieldValue, x => x.AsString(200).NotNullable());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsValue x) => x.Order, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.CustomFieldsValue x) => x.DateChange, x => x.AsInt32().NotNullable());
            }
        }
    }
}
