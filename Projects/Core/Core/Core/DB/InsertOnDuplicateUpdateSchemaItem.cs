using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.Core.Db
{
    using DbSchema;

    [Table("InsertOnDuplicate_MergeFiltersCache")]
    class InsertOnDuplicate_MergeFiltersCache
    {
        [MaxLength(128)]
        public string TableName { get; set; }

        public string FilterStr { get; set; }
    }

    class InsertOnDuplicateUpdateSchemaItem : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            CheckTableMergeFiltersCache();
            CheckFunctionTypesDefaultValues();
            CheckFunctionMergeFilters();
            CheckProcedureCreateTableUDT();
            CheckProcedureCreateQuery();
            CheckDbTriggerUpdateTable();
            CheckDbTriggerDeleteTable();
        }

        private void CheckTableMergeFiltersCache()
        {
            if (!Schema.Table< InsertOnDuplicate_MergeFiltersCache>().Exists())
            {
                Create.Table< InsertOnDuplicate_MergeFiltersCache>().
                    WithColumn((InsertOnDuplicate_MergeFiltersCache x) => x.TableName).AsString(128).NotNullable().Indexed("TableName").
                    WithColumn((InsertOnDuplicate_MergeFiltersCache x) => x.FilterStr).AsString(int.MaxValue).NotNullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (InsertOnDuplicate_MergeFiltersCache x) => x.TableName, x => x.AsString(128).NotNullable().Indexed("TableName"));
                AddColumnIfNotExists(Schema, (InsertOnDuplicate_MergeFiltersCache x) => x.FilterStr, x => x.AsString(int.MaxValue).NotNullable());

                if (!Schema.Table<InsertOnDuplicate_MergeFiltersCache>().Index("TableName").Exists())
                    Create.Index("TableName").OnTable(GetTableName<InsertOnDuplicate_MergeFiltersCache>()).OnColumn(GetColumnName((InsertOnDuplicate_MergeFiltersCache x) => x.TableName));
            }
        }

        private void CheckFunctionTypesDefaultValues()
        {
            var functionName = "InsertOnDuplicate_TypesDefaultValues";
            var functionBody = $@"
CREATE FUNCTION [dbo].[{functionName}]()
RETURNS @TypesDefaultValues TABLE (
    TypeName nvarchar(128) NOT NULL, 
    Value nvarchar(100)
)
AS BEGIN
	INSERT INTO @TypesDefaultValues (TypeName, Value) VALUES
	('bit', '0'), ('tinyint', '0'), ('smallint', '0'), ('int', '0'), ('bigint', '0'), ('numeric', '0'), ('decimal', '0'), ('smallmoney', '0'), ('money', '0'),
	('float', '0'), ('real', '0'),
	('datetime', 'GETDATE()'), ('smalldatetime', 'GETDATE()'), ('date', 'GETDATE()'), ('time', 'GETDATE()'), ('datetimeoffset', '0'), ('datetime2', 'GETDATE()'),
	('char', ''''''), ('varchar', ''''''), ('text', ''''''), ('nchar', ''''''), ('nvarchar', ''''''), ('ntext', ''''''),
	('binary', ''), ('varbinary', ''), ('image', '')
	
	RETURN 
END
            ";

            IfDatabase("sqlserver").Execute.Sql($@"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{functionName}]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' )) EXEC (N'{functionBody.Replace("'", "''")}')");
        }

        private void CheckFunctionMergeFilters()
        {
            var functionName = "InsertOnDuplicate_MergeFilters";
            var functionBody = $@"
CREATE FUNCTION [dbo].[{functionName}]()
RETURNS @Result TABLE (
    TableName nvarchar(128) NOT NULL, 
    FilterStr nvarchar(MAX)
)
AS BEGIN
	DECLARE @Fields TABLE (TableName SYSNAME, IndexName SYSNAME, ColumnStr nvarchar(MAX))
	DECLARE @Fields2 TABLE (TableName SYSNAME, FilterStr nvarchar(MAX))

	INSERT INTO @Fields (TableName, IndexName, ColumnStr)
	SELECT t.name, ind.name, '(T.[' + col.name + '] = S.[' + col.name + ']' + CASE WHEN col.is_nullable <> 0 THEN ' AND S.[' + col.name + '] IS NOT NULL' ELSE '' END + ')'
	FROM sys.indexes ind 
	INNER JOIN sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id 
	INNER JOIN sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id 
	INNER JOIN sys.tables t ON ind.object_id = t.object_id 
	WHERE (ind.is_primary_key <> 0 OR ind.is_unique = 1 OR ind.is_unique_constraint = 1)-- AND t.name = 'File'
     
	INSERT INTO @Fields2 (TableName, FilterStr)
	Select Main.TableName,
		   '(' + Left(Main.FilterStr, Len(Main.FilterStr)-4) + ')' AS Filter
	From
		(
			Select distinct ST2.TableName, ST2.IndexName, 
				(
					Select ST1.ColumnStr + ' AND ' AS [text()]
					From @Fields ST1
					Where ST1.TableName = ST2.TableName AND ST1.IndexName = ST2.IndexName
					ORDER BY ST1.TableName, ST1.IndexName
					For XML PATH ('')
				) FilterStr
			From @Fields ST2
		) [Main]     
    
	INSERT INTO @Result (TableName, FilterStr)
	Select Main.TableName,
		   '(' + Left(Main.FilterStr, Len(Main.FilterStr)-3) + ')' AS Filter
	From
		(
			Select distinct ST2.TableName, 
				(
					Select ST1.FilterStr + ' OR ' AS [text()]
					From @Fields2 ST1
					Where ST1.TableName = ST2.TableName 
					ORDER BY ST1.TableName
					For XML PATH ('')
				) FilterStr
			From @Fields ST2
		) [Main]   	
	
	RETURN 
END
            ";

            IfDatabase("sqlserver").Execute.Sql($@"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{functionName}]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' )) EXEC (N'{functionBody.Replace("'", "''")}')");
        }

        private void CheckProcedureCreateTableUDT()
        {
            var procedureName = "InsertOnDuplicate_CreateTableUDT";
            var procedureBody = $@"
CREATE PROCEDURE [dbo].[{procedureName}] 
    @TableName SYSNAME, 
    @TypeName SYSNAME OUTPUT	
AS 
BEGIN
	SET NOCOUNT ON;
	SET @TypeName = N'TVP_' + @TableName;

	DECLARE @sql NVARCHAR(MAX) = N'';
	
	SELECT @sql = @sql + N',' + CHAR(13) + CHAR(10) + CHAR(9) 
		+ QUOTENAME(c.name) + ' '
		+ s.name + 
		CASE 
			WHEN LOWER(s.name) LIKE '%char' THEN 
			'(' + CASE WHEN c.max_length = -1 THEN 'MAX' ELSE
				CONVERT(VARCHAR(12), 
											c.max_length/(CASE LOWER(LEFT(s.name, 1)) WHEN N'n' THEN 2 ELSE 1 END)
										) 
				END +
			+ ')' 
			WHEN LOWER(s.name) LIKE '%binary' THEN 
			'(' + CASE WHEN c.max_length = -1 THEN 'MAX' ELSE
				CONVERT(VARCHAR(12), 
											c.max_length/(CASE LOWER(LEFT(s.name, 1)) WHEN N'n' THEN 2 ELSE 1 END)
										) 
				END +
			+ ')' 
			ELSE
				CASE WHEN s.name = 'decimal' OR s.name = 'numeric' THEN 
					CASE WHEN c.[precision] > 0 AND c.[scale] > 0 THEN '(' + convert(nvarchar(10), c.[precision]) + ', ' + convert(nvarchar(10), c.[scale]) + ')'
						 WHEN c.[precision] > 0 THEN '(' + convert(nvarchar(10), c.[precision]) + ')'
						 ELSE '' 
					END
				ELSE '' END
			END + 
			' ' + CASE WHEN c.is_nullable = 1 THEN 'NULL' ELSE 'NOT NULL' END +
			' ' + CASE WHEN c.default_object_id = 0 THEN 
					CASE WHEN c.is_nullable = 1 THEN '' ELSE 'DEFAULT (' + ISNULL((SELECT Value FROM dbo.InsertOnDuplicate_TypesDefaultValues() WHERE TypeName = s.name), 'NULL')  + ')' END
				  ELSE 
					'DEFAULT ' + object_definition(c.default_object_id) 
				  END
			-- need much more conditionals here for other data types
		FROM sys.columns AS c
		INNER JOIN sys.types AS s
		ON c.system_type_id = s.system_type_id
		AND c.user_type_id = s.user_type_id
		WHERE c.[object_id] = OBJECT_ID(@TableName);
	    
	IF EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = @TypeName AND ss.name = N'dbo')
	BEGIN
		DECLARE @SQLDROP AS NVARCHAR(MAX) = N'DROP TYPE ' + @TypeName; 
		EXEC sp_executesql @SQLDROP
	END

	SET @sql = N'CREATE TYPE ' + @TypeName
		+ ' AS TABLE ' + CHAR(13) + CHAR(10) + '(' + STUFF(@sql, 1, 1, '')
		+ CHAR(13) + CHAR(10) + ');';

	PRINT @SQL;
	EXEC sp_executesql @sql;

	DELETE FROM dbo.InsertOnDuplicate_MergeFiltersCache WHERE TableName = @TableName;
END
            ";

            IfDatabase("sqlserver").Execute.Sql($@"IF NOT EXISTS (SELECT * FROM sys.objects WHERE name='{procedureName}') EXEC (N'{procedureBody.Replace("'", "''")}')");
        }

        private void CheckProcedureCreateQuery()
        {
            var procedureName = "InsertOnDuplicate_CreateQuery";
            var procedureBody = $@"
CREATE PROCEDURE [dbo].[InsertOnDuplicate_CreateQuery] 
	@TableName SYSNAME,
	@InsertData NVARCHAR(MAX),
	@UpdateData NVARCHAR(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE	@TableName_ sysname = @TableName;
	DECLARE	@InsertData_ NVARCHAR(MAX) = @InsertData;
	DECLARE	@UpdateData_ NVARCHAR(MAX) = @UpdateData;

	DECLARE	@TypeName sysname = N'TVP_' + @TableName;

	----не убирать комментирование! ломается работа TransactionScope, если в нем есть вызов [InsertOnDuplicate_CreateQuery]
	--IF EXISTS(SELECT * FROM fn_my_permissions(NULL, 'DATABASE') t WHERE t.permission_name='CREATE TYPE')
	--BEGIN
	--   EXEC	[dbo].[InsertOnDuplicate_CreateTableUDT]
	--	  @TableName = @TableName_,
	--	  @TypeName = @TypeName OUTPUT
	--END
	
	--Генерация условия для проверки уникальности строки в MERGE
	DECLARE	@FilterStr nvarchar(max) = (SELECT t.FilterStr FROM dbo.InsertOnDuplicate_MergeFiltersCache t WHERE t.TableName=@TableName_);
	IF @FilterStr IS NULL
	BEGIN
		SET @FilterStr = (SELECT t.FilterStr FROM dbo.InsertOnDuplicate_MergeFilters() t WHERE t.TableName=@TableName)

		IF @FilterStr IS NOT NULL
		BEGIN
			INSERT INTO InsertOnDuplicate_MergeFiltersCache (TableName, FilterStr)
			VALUES (@TableName_, @FilterStr);
		END;
	END;
	
	--Генерация INSERT полей.
	DECLARE @Fields TABLE (TableName SYSNAME, ColumnStr SYSNAME, IsIdentity bit, DefaultValue NVARCHAR(100))
	INSERT INTO @Fields (TableName, ColumnStr, IsIdentity, DefaultValue)
	SELECT t.name, col.name, col.is_identity, ISNULL('''' + def.Value + '''', 'NULL')
	FROM sys.tables t 
	INNER JOIN sys.columns col ON t.object_id = col.object_id
	INNER JOIN sys.types AS s ON col.system_type_id = s.system_type_id AND col.user_type_id = s.user_type_id
	LEFT JOIN dbo.InsertOnDuplicate_TypesDefaultValues() AS def ON def.TypeName = s.name
	
	DECLARE @FieldsInsertWithIdentity nvarchar(max) = (Select '(' + Left(Main.FilterStr, Len(Main.FilterStr)-1) + ')'
							From (
								Select distinct ST2.TableName, (
									Select '[' + ST1.ColumnStr + '], ' AS [text()]
									From @Fields ST1
									Where ST1.TableName = ST2.TableName 
									ORDER BY ST1.TableName
									For XML PATH ('')
								) FilterStr
							From @Fields ST2) [Main]     
						WHERE Main.TableName=@TableName_)
	DECLARE @FieldsInsertWithoutIdentity nvarchar(max) = (Select '(' + Left(Main.FilterStr, Len(Main.FilterStr)-1) + ')'
							From (
								Select distinct ST2.TableName, (
									Select '[' + ST1.ColumnStr + '], ' AS [text()]
									From @Fields ST1
									Where ST1.TableName = ST2.TableName AND ST1.IsIdentity = 0
									ORDER BY ST1.TableName
									For XML PATH ('')
								) FilterStr
							From @Fields ST2) [Main]     
						WHERE Main.TableName=@TableName_)
	DECLARE @FieldsUpdateWithIdentity nvarchar(max) = (Select '(' + Left(Main.FilterStr, Len(Main.FilterStr)-1) + ')'
							From (
								Select distinct ST2.TableName, (
									Select 'S.[' + ST1.ColumnStr + '], ' AS [text()]
									From @Fields ST1
									Where ST1.TableName = ST2.TableName 
									ORDER BY ST1.TableName
									For XML PATH ('')
								) FilterStr
							From @Fields ST2) [Main]     
						WHERE Main.TableName=@TableName_)
	DECLARE @FieldsUpdateWithoutIdentity nvarchar(max) = (Select '(' + Left(Main.FilterStr, Len(Main.FilterStr)-1) + ')'
							From (
								Select distinct ST2.TableName, (
									Select 'S.[' + ST1.ColumnStr + '], ' AS [text()]
									From @Fields ST1
									Where ST1.TableName = ST2.TableName AND ST1.IsIdentity = 0 
									ORDER BY ST1.TableName
									For XML PATH ('')
								) FilterStr
							From @Fields ST2) [Main]     
						WHERE Main.TableName=@TableName_)
						
	--Находим IDENTITY поле, если оно есть.
	DECLARE @FieldIdentity SYSNAME = NULL;
	DECLARE @FieldIdentityDefaultValue NVARCHAR(100) = NULL;
	SELECT @FieldIdentity = ColumnStr, @FieldIdentityDefaultValue = DefaultValue FROM @Fields WHERE IsIdentity <> 0 AND TableName=@TableName_;

	if @FilterStr is null print '0';
	if @FilterStr is not null print '1';

	print @TypeName;
	print @InsertData_;
	print @TableName_;
	print @FieldIdentity;
	print @FilterStr;
	print @FieldsUpdateWithIdentity;
	
	--Непосредственно запрос.	
	DECLARE @SQL NVARCHAR(MAX) = N'' + CHAR(13) + CHAR(10) + 
									'SET DATEFORMAT ymd;' + CHAR(13) + CHAR(10) + 
									'DECLARE @CountRows int = 0;' + CHAR(13) + CHAR(10) + 
									'DECLARE @ErrorMessage NVARCHAR(4000);' + CHAR(13) + CHAR(10) + 
									'DECLARE @ErrorSeverity INT;' + CHAR(13) + CHAR(10) + 
									'DECLARE @ErrorState INT;' + CHAR(13) + CHAR(10) + 
									'DECLARE @t ' + @TypeName + ';' + CHAR(13) + CHAR(10) + 
									'SET NOCOUNT ON;'  + CHAR(13) + CHAR(10) +
									@InsertData_ + CHAR(13) + CHAR(10) + 
									'SET NOCOUNT OFF;'  + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
									
									CASE WHEN @FieldIdentity IS NULL THEN '' ELSE 
										'SET IDENTITY_INSERT [' + @TableName_ + '] ON;' + CHAR(13) + CHAR(10) +
										'BEGIN TRY' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
										
										'	MERGE [' + @TableName_ + '] AS T' + CHAR(13) + CHAR(10) +
										'	USING (SELECT DISTINCT * FROM @t WHERE NOT [' + @FieldIdentity + '] = (' + @FieldIdentityDefaultValue + ')) AS S' + CHAR(13) + CHAR(10) +
										'		ON (' + @FilterStr + ')' + CHAR(13) + CHAR(10) +
										'	WHEN NOT MATCHED BY TARGET THEN INSERT ' + ISNULL(@FieldsInsertWithIdentity, '') + ' VALUES ' + ISNULL(@FieldsUpdateWithIdentity, '') + CHAR(13) + CHAR(10) + 
										'	WHEN MATCHED THEN UPDATE SET ' + @UpdateData_ + ';' + CHAR(13) + CHAR(10) +
										'	SET @CountRows = @CountRows + @@ROWCOUNT;' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
										
										'END TRY' + CHAR(13) + CHAR(10) +
										'BEGIN CATCH' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
										
										'	SET @ErrorMessage = ERROR_MESSAGE(); ' + CHAR(13) + CHAR(10) +
										'	SET @ErrorSeverity = ERROR_SEVERITY();' + CHAR(13) + CHAR(10) +
										'	SET @ErrorState = ERROR_STATE();' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
										
										'	SET IDENTITY_INSERT [' + @TableName_ + '] OFF;' + CHAR(13) + CHAR(10) +
										'	RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
										
										'END CATCH' + CHAR(13) + CHAR(10) + 
										'SET IDENTITY_INSERT [' + @TableName_ + '] OFF;' + CHAR(13) + CHAR(10)	+ CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) 
									END +
									
									--Теперь добавляем блок для вставки данных БЕЗ identity столбца.
									'BEGIN TRY' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
									
									'	MERGE [' + @TableName_ + '] AS T' + CHAR(13) + CHAR(10) +
									'	USING (SELECT DISTINCT * FROM @t ' + CASE WHEN @FieldIdentity IS NULL THEN '' ELSE ' WHERE [' + @FieldIdentity + '] = (' + @FieldIdentityDefaultValue + ')' END + ') AS S' + CHAR(13) + CHAR(10) +
									'		ON (' + @FilterStr + ')' + CHAR(13) + CHAR(10) +
									'	WHEN NOT MATCHED BY TARGET THEN INSERT ' + ISNULL(@FieldsInsertWithoutIdentity, '') + ' VALUES ' + ISNULL(@FieldsUpdateWithoutIdentity, '') + CHAR(13) + CHAR(10) + 
									'	WHEN MATCHED THEN UPDATE SET ' + @UpdateData_ + ';' + CHAR(13) + CHAR(10) + 
									'	SET @CountRows = @CountRows + @@ROWCOUNT;' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
									
									'END TRY' + CHAR(13) + CHAR(10) +
									'BEGIN CATCH' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
									
									'	SET @ErrorMessage = ERROR_MESSAGE(); ' + CHAR(13) + CHAR(10) +
									'	SET @ErrorSeverity = ERROR_SEVERITY();' + CHAR(13) + CHAR(10) +
									'	SET @ErrorState = ERROR_STATE();' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
									
									'	RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
									
									'END CATCH' + CHAR(13) + CHAR(10) 
									;
				
	SELECT @SQL AS Query;	
END
            ";

            IfDatabase("sqlserver").Execute.Sql($@"IF NOT EXISTS (SELECT * FROM sys.objects WHERE name='{procedureName}') EXEC (N'{procedureBody.Replace("'", "''")}')");
        }

        private void CheckDbTriggerUpdateTable()
        {
            var triggerName = "DB_InsertOnDuplicate_TableUDT";
            var triggerBody = $@"
CREATE TRIGGER [{triggerName}] ON DATABASE 
	FOR ALTER_TABLE, CREATE_TABLE
AS 
BEGIN
	IF IS_MEMBER ('db_owner') = 0
	BEGIN
	   PRINT 'Недостаточно разрешений для управления типами UDT!' 
	   ROLLBACK TRANSACTION
	END
	
	DECLARE @TableName SYSNAME = EVENTDATA().value('(/EVENT_INSTANCE/ObjectName)[1]','SYSNAME');
	DECLARE @TypeName SYSNAME;

	BEGIN TRY
		EXEC	[dbo].[InsertOnDuplicate_CreateTableUDT]
			@TableName = @TableName,
			@TypeName = @TypeName OUTPUT
	END TRY
	BEGIN CATCH
	END CATCH

	BEGIN TRY
	   DELETE FROM InsertOnDuplicate_MergeFiltersCache

	   INSERT INTO InsertOnDuplicate_MergeFiltersCache (TableName, FilterStr)
	   SELECT d.TableName, d.FilterStr FROM dbo.InsertOnDuplicate_MergeFilters() d
	END TRY
	BEGIN CATCH
	END CATCH
END;
            ";

            IfDatabase("sqlserver").Execute.Sql($@"IF NOT EXISTS (select * from sys.triggers where [name] = '{triggerName}' AND [parent_class] = 0) EXEC (N'{triggerBody.Replace("'", "''")}')");
        }

        private void CheckDbTriggerDeleteTable()
        {
            var triggerName = "DB_InsertOnDuplicate_TableUDT_Remove";
            var triggerBody = $@"
CREATE TRIGGER [{triggerName}] ON DATABASE 
	FOR DROP_TABLE
AS 
BEGIN
	IF IS_MEMBER ('db_owner') = 0
	BEGIN
	   PRINT 'Недостаточно разрешений для управления типами UDT!' 
	   ROLLBACK TRANSACTION
	END

	DECLARE @TableName SYSNAME = EVENTDATA().value('(/EVENT_INSTANCE/ObjectName)[1]','SYSNAME');
	DECLARE @TypeName SYSNAME = N'TVP_' + @TableName;

	BEGIN TRY
		IF EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = @TypeName AND ss.name = N'dbo')
		BEGIN
			DECLARE @SQLDROP AS NVARCHAR(MAX) = N'DROP TYPE ' + @TypeName; 
			EXEC sp_executesql @SQLDROP
		END
	END TRY
	BEGIN CATCH
	END CATCH
END;
            ";

            IfDatabase("sqlserver").Execute.Sql($@"IF NOT EXISTS (select * from sys.triggers where [name] = '{triggerName}' AND [parent_class] = 0) EXEC (N'{triggerBody.Replace("'", "''")}')");
        }

    }
}
