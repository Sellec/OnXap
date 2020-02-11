namespace OnXap.Messaging.DB
{
    using Core.DbSchema;

    class MessageSubscriptionSchemaItem : DbSchemaItemRegular
    {
        public MessageSubscriptionSchemaItem() : base()
        {

        }

        public override void Down()
        {
        }

        public override void Up()
        {
            if (!Schema.Table<MessageSubscription>().Exists())
            {
                Create.Table<MessageSubscription>().
                    WithColumn((MessageSubscription x) => x.IdSubscription).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((MessageSubscription x) => x.NameSubscription).AsString(200).NotNullable().
                    WithColumn((MessageSubscription x) => x.IsHidden).AsBoolean().NotNullable().
                    WithColumn((MessageSubscription x) => x.IsEnabled).AsBoolean().NotNullable().
                    WithColumn((MessageSubscription x) => x.UniqueKey).AsString(200).Nullable();

                IfDatabase("sqlserver").Execute.Sql($"CREATE UNIQUE NONCLUSTERED INDEX [UniqueKey] ON [{FluentMigratorTableExtensions.GetTableName<MessageSubscription>()}] ([{FluentMigratorColumnExtensions.GetColumnName((MessageSubscription x) => x.UniqueKey)}] ASC) WHERE ([{FluentMigratorColumnExtensions.GetColumnName((MessageSubscription x) => x.UniqueKey)}] IS NOT NULL);");
            }
            else
            {
                AddColumnIfNotExists(Schema, (MessageSubscription x) => x.IdSubscription, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (MessageSubscription x) => x.NameSubscription, x => x.AsString(200).NotNullable());
                AddColumnIfNotExists(Schema, (MessageSubscription x) => x.IsHidden, x => x.AsBoolean().NotNullable());
                AddColumnIfNotExists(Schema, (MessageSubscription x) => x.IsEnabled, x => x.AsBoolean().NotNullable());
                AddColumnIfNotExists(Schema, (MessageSubscription x) => x.UniqueKey, x => x.AsString(200).Nullable());

                if (!Schema.Table<MessageSubscription>().Index("UniqueKey").Exists())
                    IfDatabase("sqlserver").Execute.Sql($"CREATE UNIQUE NONCLUSTERED INDEX [UniqueKey] ON [{FluentMigratorTableExtensions.GetTableName<MessageSubscription>()}] ([{FluentMigratorColumnExtensions.GetColumnName((MessageSubscription x) => x.UniqueKey)}] ASC) WHERE ([{FluentMigratorColumnExtensions.GetColumnName((MessageSubscription x) => x.UniqueKey)}] IS NOT NULL);");
            }
        }
    }
}
