using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;

namespace OnXap.TaskSheduling.Db
{
    using Core.DbSchema;

    class TaskSchema : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            var isTableExists = Schema.Table<Task>().Exists();
            var tableName = GetTableName<Task>();

            if (!isTableExists)
            {
                Create.Table<Task>().
                    WithColumn((Task x) => x.Id).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((Task x) => x.Name).AsString(200).NotNullable().
                    WithColumn((Task x) => x.Description).AsString(int.MaxValue).Nullable().
                    WithColumn((Task x) => x.UniqueKey).AsString(300).NotNullable().
                    WithColumn((Task x) => x.IsEnabled).AsBoolean().Nullable();

            }
            else
            {
                AddColumnIfNotExists(Schema, (Task x) => x.Id, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (Task x) => x.Name, x => x.AsString(200).NotNullable());
                AddColumnIfNotExists(Schema, (Task x) => x.Description, x => x.AsString(int.MaxValue).Nullable());
                AddColumnIfNotExists(Schema, (Task x) => x.UniqueKey, x => x.AsString(300).NotNullable());
                AddColumnIfNotExists(Schema, (Task x) => x.IsEnabled, x => x.AsBoolean().Nullable());
            }

            //if (!isTableExists || !Schema.Table<Task>().Constraint("FK_Journal_ItemLink").Exists())
            //    Create.ForeignKey("FK_Journal_ItemLink").
            //        FromTable(GetTableName<Task>()).ForeignColumn(GetColumnName((Task x) => x.ItemLinkId)).
            //        ToTable(GetTableName<ItemLink>()).PrimaryColumn(GetColumnName((ItemLink x) => x.LinkId)).
            //        OnDelete(System.Data.Rule.Cascade);
        }
    }
}
