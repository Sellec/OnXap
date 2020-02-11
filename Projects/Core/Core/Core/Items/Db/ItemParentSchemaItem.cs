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
            if (!Schema.Table<ItemParent>().Exists())
            {
                Create.Table<ItemParent>().
                    WithColumn((ItemParent x) => x.IdModule).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((ItemParent x) => x.IdItem).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((ItemParent x) => x.IdItemType).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((ItemParent x) => x.IdParentItem).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((ItemParent x) => x.IdLevel).AsInt32().NotNullable().WithDefaultValue(0);

                Create.PrimaryKey("ItemParentKey").OnTable(FluentMigratorTableExtensions.GetTableName<ItemParent>()).Columns(
                    FluentMigratorColumnExtensions.GetColumnName((ItemParent x) => x.IdModule),
                    FluentMigratorColumnExtensions.GetColumnName((ItemParent x) => x.IdItem),
                    FluentMigratorColumnExtensions.GetColumnName((ItemParent x) => x.IdItemType),
                    FluentMigratorColumnExtensions.GetColumnName((ItemParent x) => x.IdParentItem)
                );
            }
            else
            {
                AddColumnIfNotExists(Schema, (ItemParent x) => x.IdModule, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (ItemParent x) => x.IdItem, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (ItemParent x) => x.IdItemType, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (ItemParent x) => x.IdParentItem, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (ItemParent x) => x.IdLevel, x => x.AsInt32().NotNullable().WithDefaultValue(0));

                if (!Schema.Table<ItemParent>().Index("ItemParentKey").Exists())
                    Create.PrimaryKey("ItemParentKey").OnTable(FluentMigratorTableExtensions.GetTableName<ItemParent>()).Columns(
                        FluentMigratorColumnExtensions.GetColumnName((ItemParent x) => x.IdModule),
                        FluentMigratorColumnExtensions.GetColumnName((ItemParent x) => x.IdItem),
                        FluentMigratorColumnExtensions.GetColumnName((ItemParent x) => x.IdItemType),
                        FluentMigratorColumnExtensions.GetColumnName((ItemParent x) => x.IdParentItem)
                    );
            }
        }
    }
}
