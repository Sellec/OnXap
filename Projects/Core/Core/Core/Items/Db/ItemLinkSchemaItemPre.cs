namespace OnXap.Core.Items.Db
{
    using DbSchema;

    class ItemLinkSchemaItemPre : DbSchemaItemRegular
    {
        public ItemLinkSchemaItemPre() : base(typeof(ItemLinkSchemaItemPre))
        {
        }

        public override void Down()
        {
        }

        public override void Up()
        {
            /*
             * Для переименовывания ключей.
             * */

            if (Schema.Table<ItemLink>().Exists() && !Schema.Table<ItemLink>().Index("ItemLinkKey").Exists())
            {
                var tableName = FluentMigratorTableExtensions.GetTableName<ItemLink>();

                Execute.Sql($@"
                    DECLARE @SQL VARCHAR(4000) = 'ALTER TABLE [{tableName}] DROP CONSTRAINT |ConstraintName| '
                    SET @SQL = REPLACE(@SQL, '|ConstraintName|', (SELECT [name] FROM sysobjects WHERE [xtype] = 'PK' AND [parent_obj] = OBJECT_ID('{tableName}') AND [name] LIKE 'PK__ItemLink__%' ))
                    IF LEN(@SQL) > 0 EXEC (@SQL)
                ");

            }
        }
    }
}
