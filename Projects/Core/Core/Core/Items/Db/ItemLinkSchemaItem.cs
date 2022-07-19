namespace OnXap.Core.Items.Db
{
    using DbSchema;
    using Core.Db;

    class ItemLinkSchemaItem : DbSchemaItemRegular
    {
        public ItemLinkSchemaItem() : base(typeof(UserSchemaItem))
        {
        }

        public override void Down()
        {
        }

        public override void Up()
        {
            /*
             * На этом объекте нельзя создавать внешние ключи, т.к. все запросы к объектам данного типа оборачиваются в Suppress-транзакцию.
             * */

            var isTableExists = Schema.Table<ItemLink>().Exists();
            var tableName = GetTableName<ItemLink>();

            if (!isTableExists)
            {
                Create.Table<ItemLink>().
                    WithColumn((ItemLink x) => x.ItemIdType).AsInt32().NotNullable().PrimaryKey().
                    WithColumn((ItemLink x) => x.ItemId).AsInt32().NotNullable().PrimaryKey().
                    WithColumn((ItemLink x) => x.LinkId).AsGuid().NotNullable().
                    WithColumn((ItemLink x) => x.DateCreate).AsDateTime().NotNullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (ItemLink x) => x.ItemIdType, x => x.AsInt32().NotNullable().PrimaryKey());
                AddColumnIfNotExists(Schema, (ItemLink x) => x.ItemId, x => x.AsInt32().NotNullable().PrimaryKey());
                AddColumnIfNotExists(Schema, (ItemLink x) => x.LinkId, x => x.AsGuid().NotNullable());
                AddColumnIfNotExists(Schema, (ItemLink x) => x.DateCreate, x => x.AsDateTime().NotNullable());
            }

            //if (!isTableExists || !Schema.Table<ItemLink>().Index("ItemLinkKey").Exists())
            //    Create.PrimaryKey("ItemLinkKey").OnTable(GetTableName<ItemLink>()).Columns(
            //        GetColumnName((ItemLink x) => x.ItemIdType),
            //        GetColumnName((ItemLink x) => x.ItemId),
            //        GetColumnName((ItemLink x) => x.ItemKey)
            //    );

            if (!isTableExists || !Schema.Table<ItemLink>().Index("LinkIdKey").Exists())
                Create.Index("LinkIdKey").OnTable(GetTableName<ItemLink>()).
                    OnColumn(GetColumnName((ItemLink x) => x.LinkId)).Ascending().
                    WithOptions().Unique();
        }
    }
}
