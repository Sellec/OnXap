namespace OnXap.Core.Db
{
    using DbSchema;

    class ItemTypeSchemaItem : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            if (!Schema.Table<ItemType>().Exists())
            {
                Create.Table<ItemType>().
                    WithColumn((ItemType x) => x.IdItemType).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((ItemType x) => x.NameItemType).AsString(200).NotNullable().
                    WithColumn((ItemType x) => x.UniqueKey).AsString(200).NotNullable().Unique("UniqueKey");
            }
            else
            {
                AddColumnIfNotExists(Schema, (ItemType x) => x.IdItemType, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (ItemType x) => x.NameItemType, x => x.AsString(200).NotNullable());
                AddColumnIfNotExists(Schema, (ItemType x) => x.UniqueKey, x => x.AsString(200).NotNullable().Unique("UniqueKey"));
            }
        }
    }
}
