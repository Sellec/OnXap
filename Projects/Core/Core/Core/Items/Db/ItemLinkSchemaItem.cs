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

            if (!Schema.Table<ItemLink>().Exists())
            {
                Create.Table<ItemLink>().
                    WithColumn((ItemLink x) => x.ItemIdType).AsInt32().NotNullable().
                    WithColumn((ItemLink x) => x.ItemId).AsInt32().NotNullable().
                    WithColumn((ItemLink x) => x.ItemKey).AsString(200).NotNullable().
                    WithColumn((ItemLink x) => x.LinkId).AsGuid().NotNullable().
                    WithColumn((ItemLink x) => x.DateCreate).AsDateTime().NotNullable();

                Create.PrimaryKey("ItemLinkKey").OnTable(GetTableName<ItemLink>()).Columns(
                    GetColumnName((ItemLink x) => x.ItemIdType),
                    GetColumnName((ItemLink x) => x.ItemId),
                    GetColumnName((ItemLink x) => x.ItemKey)
                );

                Create.Index("LinkIdKey").OnTable(GetTableName<ItemLink>()).
                    OnColumn(GetColumnName((ItemLink x) => x.LinkId)).Ascending().
                    WithOptions().Unique();
            }
            else
            {
                AddColumnIfNotExists(Schema, (ItemLink x) => x.ItemIdType, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (ItemLink x) => x.ItemId, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (ItemLink x) => x.ItemKey, x => x.AsString(200).NotNullable());
                AddColumnIfNotExists(Schema, (ItemLink x) => x.LinkId, x => x.AsGuid().NotNullable());
                AddColumnIfNotExists(Schema, (ItemLink x) => x.DateCreate, x => x.AsDateTime().NotNullable());

                var tableName = GetTableName<ItemLink>();

                if (!Schema.Table<ItemLink>().Index("ItemLinkKey").Exists())
                {
                    Execute.Sql($@"
                        DECLARE @SQL VARCHAR(4000) = 'ALTER TABLE [{tableName}] DROP CONSTRAINT |ConstraintName| '
                        SET @SQL = REPLACE(@SQL, '|ConstraintName|', (SELECT [name] FROM sysobjects WHERE [xtype] = 'PK' AND [parent_obj] = OBJECT_ID('{tableName}') AND [name] LIKE 'PK__%' ))
                        IF LEN(@SQL) > 0 EXEC (@SQL)
                    ");

                    Create.PrimaryKey("ItemLinkKey").OnTable(GetTableName<ItemLink>()).Columns(
                        GetColumnName((ItemLink x) => x.ItemIdType),
                        GetColumnName((ItemLink x) => x.ItemId),
                        GetColumnName((ItemLink x) => x.ItemKey)
                    );
                }

                if (Schema.Table<ItemLink>().Index("IX_ItemLink_Column").Exists())
                {
                    Execute.Sql($@"EXEC sp_rename N'{tableName}.IX_ItemLink_Column', N'LinkIdKey', N'INDEX';");
                }
                else if (!Schema.Table<ItemLink>().Index("LinkIdKey").Exists())
                {
                    Create.Index("LinkIdKey").OnTable(GetTableName<ItemLink>()).
                            OnColumn(GetColumnName((ItemLink x) => x.LinkId)).Ascending().
                            WithOptions().Unique();
                }

                if (Schema.Table<ItemLink>().Constraint("FK_ItemLink_User").Exists())
                    Delete.ForeignKey("FK_ItemLink_User").OnTable(GetTableName<ItemLink>());

                if (Schema.Table<ItemLink>().Column("IdUser").Exists())
                    Delete.Column("IdUser").FromTable(GetTableName<ItemLink>());
            }
        }
    }
}
