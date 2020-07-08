using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;

namespace OnXap.TaskSheduling.Db
{
    using Core.DbSchema;

    class TaskScheduleSchema : DbSchemaItemRegular
    {
        public TaskScheduleSchema(): base(typeof(TaskSchema))
        {
        }

        public override void Down()
        {
        }

        public override void Up()
        {
            var isTableExists = Schema.Table<TaskSchedule>().Exists();
            var tableName = GetTableName<TaskSchedule>();

            if (!isTableExists)
            {
                Create.Table<TaskSchedule>().
                    WithColumn((TaskSchedule x) => x.Id).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((TaskSchedule x) => x.IdTask).AsInt32().NotNullable().
                    WithColumn((TaskSchedule x) => x.Cron).AsString(200).Nullable().
                    WithColumn((TaskSchedule x) => x.DateTimeFixed).AsDateTimeOffset().Nullable().
                    WithColumn((TaskSchedule x) => x.IsEnabled).AsBoolean().NotNullable();

            }
            else
            {
                AddColumnIfNotExists(Schema, (TaskSchedule x) => x.Id, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (TaskSchedule x) => x.IdTask, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (TaskSchedule x) => x.Cron, x => x.AsString(200).Nullable());
                AddColumnIfNotExists(Schema, (TaskSchedule x) => x.DateTimeFixed, x => x.AsDateTimeOffset().Nullable());
                AddColumnIfNotExists(Schema, (TaskSchedule x) => x.IsEnabled, x => x.AsBoolean().NotNullable());
            }

            if (!isTableExists || !Schema.Table<TaskSchedule>().Constraint("FK_Task_TaskSchedule").Exists())
                Create.ForeignKey("FK_Task_TaskSchedule").
                    FromTable(GetTableName<TaskSchedule>()).ForeignColumn(GetColumnName((TaskSchedule x) => x.IdTask)).
                    ToTable(GetTableName<Task>()).PrimaryColumn(GetColumnName((Task x) => x.Id)).
                    OnDelete(System.Data.Rule.Cascade);
        }
    }
}
