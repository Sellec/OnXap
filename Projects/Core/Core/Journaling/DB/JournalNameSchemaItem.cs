using FluentMigrator.SqlServer;

namespace OnXap.Journaling.DB
{
    using Core.DbSchema;

    class JournalNameSchemaItem : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            if (!Schema.Table<JournalNameDAO>().Exists())
            {
                Create.Table<JournalNameDAO>().
                    WithColumn((JournalNameDAO x) => x.IdJournal).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((JournalNameDAO x) => x.IdJournalType).AsInt32().NotNullable().
                    WithColumn((JournalNameDAO x) => x.Name).AsString(150).NotNullable().
                    WithColumn((JournalNameDAO x) => x.UniqueKey).AsString(600).Nullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (JournalNameDAO x) => x.IdJournal, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (JournalNameDAO x) => x.IdJournalType, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (JournalNameDAO x) => x.Name, x => x.AsString(150).NotNullable());
                AddColumnIfNotExists(Schema, (JournalNameDAO x) => x.UniqueKey, x => x.AsString(600).Nullable());
            }

            if (!Schema.Table<JournalNameDAO>().Exists() || !Schema.Table<JournalNameDAO>().Index($"t{GetTableName<JournalNameDAO>()}_iUniqueKey").Exists())
                Create.Index($"t{GetTableName<JournalNameDAO>()}_iUniqueKey").OnTable(GetTableName<JournalNameDAO>()).
                    OnColumn(GetColumnName((JournalNameDAO x) => x.UniqueKey)).Ascending().
                    WithOptions().UniqueNullsNotDistinct();

        }
    }
}
