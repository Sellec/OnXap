using FluentMigrator.SqlServer;

namespace OnXap.Messaging.DbSchema
{
    using Core.DbSchema;

    class MessageSubscription : DbSchemaItemRegular
    {
        public MessageSubscription() : base()
        {

        }

        public override void Down()
        {
        }

        public override void Up()
        {
            if (!Schema.Table<DB.MessageSubscription>().Exists())
            {
                Create.Table<DB.MessageSubscription>().
                    WithColumn((DB.MessageSubscription x) => x.IdSubscription).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((DB.MessageSubscription x) => x.NameSubscription).AsString(200).NotNullable().
                    WithColumn((DB.MessageSubscription x) => x.IsHidden).AsBoolean().NotNullable().
                    WithColumn((DB.MessageSubscription x) => x.IsEnabled).AsBoolean().NotNullable().
                    WithColumn((DB.MessageSubscription x) => x.UniqueKey).AsString(200).Nullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (DB.MessageSubscription x) => x.IdSubscription, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (DB.MessageSubscription x) => x.NameSubscription, x => x.AsString(200).NotNullable());
                AddColumnIfNotExists(Schema, (DB.MessageSubscription x) => x.IsHidden, x => x.AsBoolean().NotNullable());
                AddColumnIfNotExists(Schema, (DB.MessageSubscription x) => x.IsEnabled, x => x.AsBoolean().NotNullable());
                AddColumnIfNotExists(Schema, (DB.MessageSubscription x) => x.UniqueKey, x => x.AsString(200).Nullable());
            }

            if (!Schema.Table<DB.MessageSubscription>().Exists() || !Schema.Table<DB.MessageSubscription>().Index($"t{GetTableName<DB.MessageSubscription>()}_iUniqueKey").Exists())
                Create.Index($"t{GetTableName<DB.MessageSubscription>()}_iUniqueKey").OnTable(GetTableName<DB.MessageSubscription>()).
                    OnColumn(GetColumnName((DB.MessageSubscription x) => x.UniqueKey)).Ascending().
                    WithOptions().UniqueNullsNotDistinct();
        }
    }
}
