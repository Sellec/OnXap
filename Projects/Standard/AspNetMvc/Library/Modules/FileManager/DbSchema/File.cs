using FluentMigrator.SqlServer;

namespace OnXap.Modules.FileManager.DbSchema
{
    using Core.DbSchema;
    using Modules.ItemsCustomize.DB;

    public class File : DbSchemaItemRegular
    {
        public File() : base(typeof(FileRemoveQueue))
        {
        }

        public override void Down()
        {
        }

        public override void Up()
        {
            CheckTable();
            CheckProcedurePlaceFileIntoQueue();
            CheckProcedureFileCountUpdate();
        }

        private void CheckTable()
        { 
            var isTableExists = Schema.Table<Db.File>().Exists();

            if (!isTableExists)
            {
                Create.Table<Db.File>().
                    WithColumn((Db.File x) => x.IdFile).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((Db.File x) => x.IdModule).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((Db.File x) => x.NameFile).AsString(260).NotNullable().
                    WithColumn((Db.File x) => x.PathFile).AsString(260).NotNullable().
                    WithColumn((Db.File x) => x.UniqueKey).AsGuid().Nullable().
                    WithColumn((Db.FileCountUsage x) => x.CountUsage).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((Db.File x) => x.TypeCommon).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((Db.File x) => x.TypeConcrete).AsString(100).Nullable().
                    WithColumn((Db.File x) => x.DateChange).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((Db.File x) => x.DateExpire).AsDateTime().Nullable().
                    WithColumn((Db.File x) => x.IdUserChange).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((Db.File x) => x.IsRemoving).AsBoolean().NotNullable().WithDefaultValue(false).
                    WithColumn((Db.File x) => x.IsRemoved).AsBoolean().NotNullable().WithDefaultValue(false);
            }
            else
            {
                AddColumnIfNotExists(Schema, (Db.File x) => x.IdFile, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (Db.File x) => x.IdModule, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (Db.File x) => x.NameFile, x => x.AsString(260).NotNullable());
                AddColumnIfNotExists(Schema, (Db.File x) => x.PathFile, x => x.AsString(260).NotNullable());
                AddColumnIfNotExists(Schema, (Db.File x) => x.UniqueKey, x => x.AsGuid().Nullable());
                AddColumnIfNotExists(Schema, (Db.FileCountUsage x) => x.CountUsage, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (Db.File x) => x.TypeCommon, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (Db.File x) => x.TypeConcrete, x => x.AsString(100).Nullable());
                AddColumnIfNotExists(Schema, (Db.File x) => x.DateChange, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (Db.File x) => x.DateExpire, x => x.AsDateTime().Nullable());
                AddColumnIfNotExists(Schema, (Db.File x) => x.IdUserChange, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (Db.File x) => x.IsRemoving, x => x.AsBoolean().NotNullable().WithDefaultValue(false));
                AddColumnIfNotExists(Schema, (Db.File x) => x.IsRemoved, x => x.AsBoolean().NotNullable().WithDefaultValue(false));
            }


            if (!Schema.Table<Db.File>().Exists() || !Schema.Table<Db.File>().Index("UniqueKey").Exists())
                Create.Index("UniqueKey").OnTable(GetTableName<Db.File>()).
                    OnColumn(GetColumnName((Db.File x) => x.UniqueKey)).Ascending().
                    WithOptions().UniqueNullsNotDistinct();

            if (!isTableExists || !Schema.Table<Db.File>().Index("NCI_Removing").Exists())
                Create.Index("NCI_Removing").OnTable(GetTableName<Db.File>()).
                    OnColumn(GetColumnName((Db.File x) => x.IsRemoving)).Ascending().
                    OnColumn(GetColumnName((Db.File x) => x.IsRemoved)).Ascending().
                    Include(GetColumnName((Db.File x) => x.DateExpire));

            if (!isTableExists || !Schema.Table<Db.File>().Index("File_IsRemoved_IsRemoving_with_IdFile").Exists())
                Create.Index("File_IsRemoved_IsRemoving_with_IdFile").OnTable(GetTableName<Db.File>()).
                    OnColumn(GetColumnName((Db.File x) => x.IsRemoved)).Ascending().
                    OnColumn(GetColumnName((Db.File x) => x.IsRemoving)).Ascending().
                    Include(GetColumnName((Db.File x) => x.IdFile));
        }

        private void CheckProcedurePlaceFileIntoQueue()
        {
            var procedureName = "FileManager_PlaceFileIntoQueue";
            var procedureBody = $@"
-- =============================================
-- Description:	Файлы с истекшим сроком жизни помечаются на удаление. Все файлы, помеченные на удаление (по сроку жизни или вручную) помещаются в очередь на удаление.
-- =============================================
CREATE PROCEDURE [dbo].[{procedureName}]
AS BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

 --   --Файлы с истекшим сроком годности помечаются на удаление. До исправления ошибки с несбрасыванием Expires для CustomFields убираем удаление файлов с истекшим сроком.
	--UPDATE [{GetTableName<Db.File>()}]
	--SET [{GetColumnName((Db.File x) => x.IsRemoving)}] = 1
	--WHERE [{GetColumnName((Db.File x) => x.IsRemoved)}] = 0 AND [{GetColumnName((Db.File x) => x.IsRemoving)}] = 0 AND [{GetColumnName((Db.File x) => x.DateExpire)}] IS NOT NULL AND [{GetColumnName((Db.File x) => x.DateExpire)}] <= GETDATE()

	-- Файлы, помеченные на удаление, помещаются в очередь на удаление.
	INSERT INTO [{GetTableName<Db.FileRemoveQueue>()}] ([{GetColumnName((Db.FileRemoveQueue x) => x.IdFile)}])
	SELECT f.[{GetColumnName((Db.File x) => x.IdFile)}] 
	FROM [{GetTableName<Db.File>()}] f
	LEFT JOIN [{GetTableName<Db.FileRemoveQueue>()}] q ON f.[{GetColumnName((Db.File x) => x.IdFile)}] = q.[{GetColumnName((Db.FileRemoveQueue x) => x.IdFile)}]
	WHERE f.[{GetColumnName((Db.File x) => x.IsRemoved)}] = 0 AND f.[{GetColumnName((Db.File x) => x.IsRemoving)}] = 1 AND q.[{GetColumnName((Db.FileRemoveQueue x) => x.IdFile)}] IS NULL;

	PRINT 'В очередь на удаление помещено ' + convert(nvarchar(10), @@ROWCOUNT) + ' файлов.'
END
            ";

            IfDatabase("sqlserver").Execute.Sql($@"IF NOT EXISTS (SELECT * FROM sys.objects WHERE name='{procedureName}') EXEC (N'{procedureBody.Replace("'", "''")}')");
        }

        private void CheckProcedureFileCountUpdate()
        {
            var procedureName = "FileManager_FileCountUpdate";
            var procedureBody = $@"
-- =============================================
-- Обновление количества ссылок для таблицы 
-- =============================================
CREATE PROCEDURE [dbo].[{procedureName}]
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @sql_tables nvarchar(max) =	null;

	SELECT @sql_tables = CASE WHEN @sql_tables IS NULL THEN '' ELSE @sql_tables + CHAR(13) + CHAR(10) + '		UNION ALL' + CHAR(13) + CHAR(10) END + '		SELECT ' + c.name + ' AS IdFile, count(*) AS FileCount FROM ' + t.name + ' GROUP BY ' + c.name
	FROM sys.foreign_key_columns AS fk
	INNER JOIN sys.tables AS t ON fk.parent_object_id = t.object_id
	INNER JOIN sys.columns AS c ON fk.parent_object_id = c.object_id AND fk.parent_column_id = c.column_id
	INNER JOIN sys.columns AS c2 ON fk.referenced_object_id = c2.object_id AND fk.referenced_column_id = c2.column_id
	WHERE fk.referenced_object_id = (SELECT object_id FROM sys.tables WHERE name = 'File') AND c2.name = 'IdFile';

	IF @sql_tables IS NOT NULL
	BEGIN
		DECLARE @sql nvarchar(max) =	'UPDATE dbo.[{GetTableName<Db.File>()}]' + CHAR(13) + CHAR(10) + 
										'SET [{GetColumnName((Db.FileCountUsage x) => x.CountUsage)}] = t2.FileCount' + CHAR(13) + CHAR(10) + 
										'FROM dbo.[{GetTableName<Db.File>()}]' + CHAR(13) + CHAR(10) + 
										'INNER JOIN (' + CHAR(13) + CHAR(10) +
										'	SELECT IdFile, SUM(FileCount) AS FileCount ' + CHAR(13) + CHAR(10) + 
										'	FROM (' + CHAR(13) + CHAR(10) + 
											@sql_tables + CHAR(13) + CHAR(10) + 
											
											--Ниже - хак для CustomFields, т.к. нельзя создавать Foreign Key с условием.
										'		UNION ALL ' + CHAR(13) + CHAR(10) +
										'		SELECT cdata.[{GetColumnName((CustomFieldsData x) => x.IdFieldValue)}] AS IdFile, count(*) AS FileCount ' + CHAR(13) + CHAR(10) +
										'		FROM [{GetTableName<CustomFieldsData>()}] cdata ' + CHAR(13) + CHAR(10) +
										'		INNER JOIN [{GetTableName<CustomFieldsField>()}] cfield ON cfield.[{GetColumnName((CustomFieldsField x) => x.IdField)}] = cdata.{GetColumnName((CustomFieldsData x) => x.IdField)} ' + CHAR(13) + CHAR(10) +
										'		WHERE cfield.[{GetColumnName((CustomFieldsField x) => x.IdFieldType)}] in ({CustomFieldsFileTypes.FileFieldType.IdFieldTypeFile}, {CustomFieldsFileTypes.FileImageFieldType.IdFieldTypeImage}) ' + CHAR(13) + CHAR(10) +
										'		GROUP BY cdata.[{GetColumnName((CustomFieldsData x) => x.IdFieldValue)}] ' + CHAR(13) + CHAR(10) +
										'	) t' + CHAR(13) + CHAR(10) +
										'	GROUP BY IdFile' + CHAR(13) + CHAR(10) +
										') t2 ON t2.IdFile = dbo.[{GetTableName<Db.File>()}].[{GetColumnName((Db.File x) => x.IdFile)}]';

		print @sql;
		EXEC sp_executesql @sql;
	END;
END
            ";

            IfDatabase("sqlserver").Execute.Sql($@"IF NOT EXISTS (SELECT * FROM sys.objects WHERE name='{procedureName}') EXEC (N'{procedureBody.Replace("'", "''")}')");
        }
    }
}
