using FluentMigrator.SqlServer;

namespace OnXap.Modules.Routing.DbSchema
{
    using Core.DbSchema;

    class Routing : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            CheckTable();
            CheckTrigger();
        }

        private void CheckTable()
        { 
            var isTableExists = Schema.Table<Db.Routing>().Exists();

            if (!isTableExists)
            {
                Create.Table<Db.Routing>().
                    WithColumn((Db.Routing x) => x.IdRoute).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((Db.Routing x) => x.IdRoutingType).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((Db.Routing x) => x.IdModule).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((Db.Routing x) => x.IdItem).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((Db.Routing x) => x.IdItemType).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((Db.Routing x) => x.Action).AsString(200).NotNullable().WithDefaultValue("").
                    WithColumn((Db.Routing x) => x.Arguments).AsString(4000).Nullable().
                    WithColumn((Db.Routing x) => x.UrlFull).AsString(500).NotNullable().WithDefaultValue("").
                    WithColumn((Db.Routing x) => x.DateChange).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((Db.Routing x) => x.IdUserChange).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((Db.Routing x) => x.IsFixedLength).AsBoolean().NotNullable().WithDefaultValue(false).
                    WithColumn((Db.Routing x) => x.UniqueKey).AsString(200).Nullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (Db.Routing x) => x.IdRoute, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (Db.Routing x) => x.IdRoutingType, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (Db.Routing x) => x.IdModule, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (Db.Routing x) => x.IdItem, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (Db.Routing x) => x.IdItemType, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (Db.Routing x) => x.Action, x => x.AsString(200).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (Db.Routing x) => x.Arguments, x => x.AsString(4000).Nullable());
                AddColumnIfNotExists(Schema, (Db.Routing x) => x.UrlFull, x => x.AsString(500).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (Db.Routing x) => x.DateChange, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (Db.Routing x) => x.IdUserChange, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (Db.Routing x) => x.IsFixedLength, x => x.AsBoolean().NotNullable().WithDefaultValue(false));
                AddColumnIfNotExists(Schema, (Db.Routing x) => x.UniqueKey, x => x.AsString(200).Nullable());
            }

            if (Schema.Table<Db.Routing>().Index("NonClusteredIndex_20180302_114957").Exists()) Delete.Index("NonClusteredIndex_20180302_114957").OnTable(GetTableName<Db.Routing>());
            if (Schema.Table<Db.Routing>().Index("NonClusteredIndex_IdTranslationType").Exists()) Delete.Index("NonClusteredIndex_IdTranslationType").OnTable(GetTableName<Db.Routing>());
            if (Schema.Table<Db.Routing>().Index("NonClusteredIndex_RoutingByUrlFull").Exists()) Delete.Index("NonClusteredIndex_RoutingByUrlFull").OnTable(GetTableName<Db.Routing>());
            if (Schema.Table<Db.Routing>().Index("urltranslation$IdModule_2").Exists()) Delete.Index("urltranslation$IdModule_2").OnTable(GetTableName<Db.Routing>());
            if (Schema.Table<Db.Routing>().Index("urltranslationUniqueKey").Exists()) Delete.Index("urltranslationUniqueKey").OnTable(GetTableName<Db.Routing>());

            if (!isTableExists || !Schema.Table<Db.Routing>().Index("IdModule_IdItem_IdItemType_UniqueKey_with_UrlFull").Exists())
                Create.Index("IdModule_IdItem_IdItemType_UniqueKey_with_UrlFull").OnTable(GetTableName<Db.Routing>()).
                    OnColumn(GetColumnName((Db.Routing x) => x.IdModule)).Ascending().
                    OnColumn(GetColumnName((Db.Routing x) => x.IdItem)).Ascending().
                    OnColumn(GetColumnName((Db.Routing x) => x.IdItemType)).Ascending().
                    OnColumn(GetColumnName((Db.Routing x) => x.UniqueKey)).Ascending().
                    Include(GetColumnName((Db.Routing x) => x.UrlFull));

            if (!isTableExists || !Schema.Table<Db.Routing>().Index("IdRoutingType_with_IdRoute_UrlFull_UniqueKey").Exists())
                Create.Index("IdRoutingType_with_IdRoute_UrlFull_UniqueKey").OnTable(GetTableName<Db.Routing>()).
                    OnColumn(GetColumnName((Db.Routing x) => x.IdRoutingType)).Ascending().
                    Include(GetColumnName((Db.Routing x) => x.IdRoute)).
                    Include(GetColumnName((Db.Routing x) => x.UrlFull)).
                    Include(GetColumnName((Db.Routing x) => x.UniqueKey));

            if (!isTableExists || !Schema.Table<Db.Routing>().Index("UrlFull_IsFixedLength_with_IdRoute_IdRoutingType_DateChange").Exists())
                Create.Index("UrlFull_IsFixedLength_with_IdRoute_IdRoutingType_DateChange").OnTable(GetTableName<Db.Routing>()).
                    OnColumn(GetColumnName((Db.Routing x) => x.UrlFull)).Ascending().
                    OnColumn(GetColumnName((Db.Routing x) => x.IsFixedLength)).Ascending().
                    Include(GetColumnName((Db.Routing x) => x.IdRoute)).
                    Include(GetColumnName((Db.Routing x) => x.IdRoutingType)).
                    Include(GetColumnName((Db.Routing x) => x.DateChange));

            if (!isTableExists || !Schema.Table<Db.Routing>().Index("IdModule_IdItem_IdItemType_Action_IdRoutingType").Exists())
                Create.Index("IdModule_IdItem_IdItemType_Action_IdRoutingType").OnTable(GetTableName<Db.Routing>()).
                    OnColumn(GetColumnName((Db.Routing x) => x.IdModule)).Ascending().
                    OnColumn(GetColumnName((Db.Routing x) => x.IdItem)).Ascending().
                    OnColumn(GetColumnName((Db.Routing x) => x.IdItemType)).Ascending().
                    OnColumn(GetColumnName((Db.Routing x) => x.Action)).Ascending().
                    OnColumn(GetColumnName((Db.Routing x) => x.IdRoutingType)).Ascending();

            if (!isTableExists || !Schema.Table<Db.Routing>().Index("UniqueKey_IdModule_IdItem_IdItemType_Action").Exists())
                IfDatabase("sqlserver").Execute.Sql($@"
                    CREATE UNIQUE NONCLUSTERED INDEX [UniqueKey_IdModule_IdItem_IdItemType_Action] ON [dbo].[{GetTableName<Db.Routing>()}]
                    (
	                    [{GetColumnName((Db.Routing x) => x.UniqueKey)}] ASC,
	                    [{GetColumnName((Db.Routing x) => x.IdModule)}] ASC,
	                    [{GetColumnName((Db.Routing x) => x.IdItem)}] ASC,
	                    [{GetColumnName((Db.Routing x) => x.IdItemType)}] ASC,
	                    [{GetColumnName((Db.Routing x) => x.Action)}] ASC
                    )
                    WHERE ([{GetColumnName((Db.Routing x) => x.UniqueKey)}] IS NOT NULL AND [{GetColumnName((Db.Routing x) => x.IdRoutingType)}]<>(2))
                ".Replace("                    ", ""));

            if (!isTableExists || !Schema.Table<Db.Routing>().Index("IdItem_IdItemType_UniqueKey").Exists())
                Create.Index("IdItem_IdItemType_UniqueKey").OnTable(GetTableName<Db.Routing>()).
                    OnColumn(GetColumnName((Db.Routing x) => x.IdItem)).Ascending().
                    OnColumn(GetColumnName((Db.Routing x) => x.IdItemType)).Ascending().
                    OnColumn(GetColumnName((Db.Routing x) => x.UniqueKey)).Ascending();

            if (!isTableExists || !Schema.Table<Db.Routing>().Constraint("FK_Routing_ItemType").Exists())
                Create.ForeignKey("FK_Routing_ItemType").
                    FromTable(GetTableName<Db.Routing>()).ForeignColumn(GetColumnName((Db.Routing x) => x.IdItemType)).
                    ToTable(GetTableName<Core.Db.ItemType>()).PrimaryColumn(GetColumnName((Core.Db.ItemType x) => x.IdItemType)).
                    OnDelete(System.Data.Rule.Cascade);
        }

        private void CheckTrigger()
        {
            var tableName = GetTableName<Db.Routing>();
            var triggerName = "UrlTranslation_SaveOldAddresses_AND_KeepMainAddressesUnique";
            var triggerBody = $@"
-- =============================================
-- Триггер разбит на 2 части.
--
-- 1. [{tableName}SaveOldAddresses]
-- При добавлении адресов любого типа, кроме 2 (2 - история адресов) дублирует эти адреса с типом 2, сохраняя историю адресов для конкретного объекта. 
-- При этом [UniqueKey] для записей с типом 2 отбрасывается.
--
-- 2. [{tableName}KeepMainAddressesUnique]
-- При добавлении адресов типа 1 (1 - основной адрес сущности) ищет адреса с аналогичным [UrlFull] для других сущностей и удаляет их. 
-- Т.о. один [UrlFull] с типом 1 может соответствовать только одной сущности. При этом другие сущности теряют свой основной адрес, но, т.к. адрес дублируется с типом 2 (см. триггер [{tableName}SaveOldAddresses]), то в случае коллизий по прежнему можно обратиться к сущности по указанному адресу.
-- =============================================
CREATE TRIGGER [dbo].[{triggerName}] ON [dbo].[{tableName}] AFTER INSERT, UPDATE
AS 
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

    -- 1. [{tableName}SaveOldAddresses]
    SET IDENTITY_INSERT dbo.[{tableName}] off;

    INSERT INTO {tableName} (
	   [{GetColumnName((Db.Routing x) => x.IdRoutingType)}],
	   [{GetColumnName((Db.Routing x) => x.IdModule)}],
	   [{GetColumnName((Db.Routing x) => x.IdItem)}],
	   [{GetColumnName((Db.Routing x) => x.IdItemType)}],
	   [{GetColumnName((Db.Routing x) => x.Action)}],
	   [{GetColumnName((Db.Routing x) => x.Arguments)}],
	   [{GetColumnName((Db.Routing x) => x.UrlFull)}],
	   [{GetColumnName((Db.Routing x) => x.DateChange)}],
	   [{GetColumnName((Db.Routing x) => x.IdUserChange)}],
	   [{GetColumnName((Db.Routing x) => x.IsFixedLength)}],
	   [{GetColumnName((Db.Routing x) => x.UniqueKey)}])
    SELECT 
	   2, 
	   i.[{GetColumnName((Db.Routing x) => x.IdModule)}],
	   i.[{GetColumnName((Db.Routing x) => x.IdItem)}],
	   i.[{GetColumnName((Db.Routing x) => x.IdItemType)}],
	   i.[{GetColumnName((Db.Routing x) => x.Action)}],
	   i.[{GetColumnName((Db.Routing x) => x.Arguments)}],
	   i.[{GetColumnName((Db.Routing x) => x.UrlFull)}],
	   i.[{GetColumnName((Db.Routing x) => x.DateChange)}],
	   i.[{GetColumnName((Db.Routing x) => x.IdUserChange)}],
	   i.[{GetColumnName((Db.Routing x) => x.IsFixedLength)}],
	   NULL
    FROM inserted i
    LEFT JOIN dbo.[{tableName}] u ON 2 = u.[{GetColumnName((Db.Routing x) => x.IdRoutingType)}] AND 
						  i.[{GetColumnName((Db.Routing x) => x.IdModule)}] = u.[{GetColumnName((Db.Routing x) => x.IdModule)}] AND 	
						  i.[{GetColumnName((Db.Routing x) => x.IdItem)}] = u.[{GetColumnName((Db.Routing x) => x.IdItem)}] AND 
						  i.[{GetColumnName((Db.Routing x) => x.IdItemType)}] = u.[{GetColumnName((Db.Routing x) => x.IdItemType)}] AND 
						  i.[{GetColumnName((Db.Routing x) => x.Action)}] = u.[{GetColumnName((Db.Routing x) => x.Action)}] AND 
						  i.[{GetColumnName((Db.Routing x) => x.Arguments)}] = u.[{GetColumnName((Db.Routing x) => x.Arguments)}] AND 
						  i.[{GetColumnName((Db.Routing x) => x.UrlFull)}] = u.[{GetColumnName((Db.Routing x) => x.UrlFull)}] AND 
						  i.[{GetColumnName((Db.Routing x) => x.IsFixedLength)}] = u.[{GetColumnName((Db.Routing x) => x.IsFixedLength)}]
    WHERE u.[{GetColumnName((Db.Routing x) => x.IdRoute)}] IS NULL

    -- 2. [{tableName}KeepMainAddressesUnique]
    DECLARE @Result VARCHAR(MAX);

    --SELECT 
    --@Result =	CASE
				--WHEN @Result IS NULL
				--THEN ''
				--ELSE @Result + ';' + CHAR(13) + CHAR(10)
			 --END + 'remove ' + convert(nvarchar(10), u.IdTranslation) + ' with ''' + u.[UrlFull] + ''' becouse i.[IdTranslation](' + convert(nvarchar(10), i.IdTranslation) + ') <> u.[IdTranslation](' + convert(nvarchar(10), u.IdTranslation) + ')'
    --FROM [{tableName}] u
    --INNER JOIN inserted i ON i.[UrlFull] = u.[UrlFull] AND i.[IdTranslation] <> u.[IdTranslation]
    --WHERE 
	   --i.[{GetColumnName((Db.Routing x) => x.IdRoutingType)}] = 1 AND 
	   --u.[{GetColumnName((Db.Routing x) => x.IdRoutingType)}] = 1 AND 
	   --u.[IdTranslation] NOT IN (
		  --SELECT MAX(i.[IdTranslation]) AS IdTranslationMax
		  --FROM [{tableName}] u
		  --INNER JOIN inserted i ON i.[UrlFull] = u.[UrlFull] AND i.[IdTranslation] <> u.[IdTranslation]
		  --WHERE i.[{GetColumnName((Db.Routing x) => x.IdRoutingType)}] = 1 AND u.[{GetColumnName((Db.Routing x) => x.IdRoutingType)}] = 1
    --)

    --print @Result

	--это убрал. пусь будут одинаковые urlfull у разных итемов, система всё равно будет брать последний зарегистрированный.
    --DELETE FROM [{tableName}]
    --FROM [{tableName}] u
    --INNER JOIN inserted i ON i.[UrlFull] = u.[UrlFull] AND i.[IdTranslation] <> u.[IdTranslation]
    --WHERE 
	   --i.[{GetColumnName((Db.Routing x) => x.IdRoutingType)}] = 1 AND 
	   --u.[{GetColumnName((Db.Routing x) => x.IdRoutingType)}] = 1 AND 
	   --u.[IdTranslation] NOT IN (
		  --SELECT MAX(i.[IdTranslation]) AS IdTranslationMax
		  --FROM [{tableName}] u
		  --INNER JOIN inserted i ON i.[UrlFull] = u.[UrlFull] AND i.[IdTranslation] <> u.[IdTranslation]
		  --WHERE i.[{GetColumnName((Db.Routing x) => x.IdRoutingType)}] = 1 AND u.[{GetColumnName((Db.Routing x) => x.IdRoutingType)}] = 1
    --)
	   
END

            ";

            IfDatabase("sqlserver").Execute.Sql($@"IF NOT EXISTS (select * from sys.triggers where [name] = '{triggerName}' AND [parent_class] = 1) EXEC (N'{triggerBody.Replace("'", "''")}')");
        }

    }
}
