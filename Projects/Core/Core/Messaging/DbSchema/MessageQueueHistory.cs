namespace OnXap.Messaging.DbSchema
{
    using Core.DbSchema;

    class MessageQueueHistory : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            var isTableExists = Schema.Table<DB.MessageQueueHistory>().Exists();

            if (!isTableExists)
            {
                Create.Table<DB.MessageQueueHistory>().
                    WithColumn((DB.MessageQueueHistory x) => x.IdQueueHistory).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((DB.MessageQueueHistory x) => x.IdQueue).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((DB.MessageQueueHistory x) => x.DateEvent).AsDateTime().NotNullable().
                    WithColumn((DB.MessageQueueHistory x) => x.EventText).AsString(500).Nullable().
                    WithColumn((DB.MessageQueueHistory x) => x.IsSuccess).AsBoolean().NotNullable().WithDefaultValue(false);
            }
            else
            {
                AddColumnIfNotExists(Schema, (DB.MessageQueueHistory x) => x.IdQueueHistory, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (DB.MessageQueueHistory x) => x.IdQueue, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (DB.MessageQueueHistory x) => x.DateEvent, x => x.AsDateTime().NotNullable());
                AddColumnIfNotExists(Schema, (DB.MessageQueueHistory x) => x.EventText, x => x.AsString(500).Nullable());
                AddColumnIfNotExists(Schema, (DB.MessageQueueHistory x) => x.IsSuccess, x => x.AsBoolean().NotNullable().WithDefaultValue(false));
            }
        }
    }
}
