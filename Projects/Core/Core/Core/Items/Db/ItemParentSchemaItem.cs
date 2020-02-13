namespace OnXap.Core.Items.Db
{
    using DbSchema;

    class ItemParentSchemaItem : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            var isTableExists = Schema.Table<ItemParent>().Exists();

            if (!isTableExists)
            {
                Create.Table<ItemParent>().
                    WithColumn((ItemParent x) => x.IdModule).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((ItemParent x) => x.IdItem).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((ItemParent x) => x.IdItemType).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((ItemParent x) => x.IdParentItem).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((ItemParent x) => x.IdLevel).AsInt32().NotNullable().WithDefaultValue(0);
            }
            else
            {
                AddColumnIfNotExists(Schema, (ItemParent x) => x.IdModule, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (ItemParent x) => x.IdItem, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (ItemParent x) => x.IdItemType, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (ItemParent x) => x.IdParentItem, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (ItemParent x) => x.IdLevel, x => x.AsInt32().NotNullable().WithDefaultValue(0));
            }

            if (!isTableExists || !Schema.Table<ItemParent>().Index("ItemParentKey").Exists())
                Create.PrimaryKey("ItemParentKey").OnTable(GetTableName<ItemParent>()).Columns(
                    GetColumnName((ItemParent x) => x.IdModule),
                    GetColumnName((ItemParent x) => x.IdItem),
                    GetColumnName((ItemParent x) => x.IdItemType),
                    GetColumnName((ItemParent x) => x.IdParentItem)
                );
        }
    }
}
