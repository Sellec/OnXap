namespace OnXap.Messaging.DbSchema
{
    using Core.DbSchema;

    class MessageQueue : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            var isTableExists = Schema.Table<DB.MessageQueue>().Exists();

            if (!isTableExists)
            {
                Create.Table<DB.MessageQueue>().
                    WithColumn((DB.MessageQueue x) => x.IdQueue).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((DB.MessageQueue x) => x.IdMessageType).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((DB.MessageQueue x) => x.Direction).AsBoolean().NotNullable().WithDefaultValue(false).
                    WithColumn((DB.MessageQueue x) => x.DateCreate).AsDateTime().NotNullable().WithDefault(FluentMigrator.SystemMethods.CurrentDateTime).
                    WithColumn((DB.MessageQueue x) => x.StateType).AsByte().NotNullable().WithDefaultValue(0).
                    WithColumn((DB.MessageQueue x) => x.State).AsString(200).Nullable().
                    WithColumn((DB.MessageQueue x) => x.IdTypeComponent).AsInt32().Nullable().
                    WithColumn((DB.MessageQueue x) => x.DateChange).AsDateTime().Nullable().
                    WithColumn((DB.MessageQueue x) => x.DateDelayed).AsDateTime().Nullable().
                    WithColumn((DB.MessageQueue x) => x.MessageInfo).AsString(int.MaxValue).Nullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (DB.MessageQueue x) => x.IdQueue, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (DB.MessageQueue x) => x.IdMessageType, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (DB.MessageQueue x) => x.Direction, x => x.AsBoolean().NotNullable().WithDefaultValue(false));
                AddColumnIfNotExists(Schema, (DB.MessageQueue x) => x.DateCreate, x => x.AsDateTime().NotNullable().WithDefault(FluentMigrator.SystemMethods.CurrentDateTime));
                AddColumnIfNotExists(Schema, (DB.MessageQueue x) => x.StateType, x => x.AsByte().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (DB.MessageQueue x) => x.State, x => x.AsString(200).Nullable());
                AddColumnIfNotExists(Schema, (DB.MessageQueue x) => x.IdTypeComponent, x => x.AsInt32().Nullable());
                AddColumnIfNotExists(Schema, (DB.MessageQueue x) => x.DateChange, x => x.AsDateTime().Nullable());
                AddColumnIfNotExists(Schema, (DB.MessageQueue x) => x.DateDelayed, x => x.AsDateTime().Nullable());
                AddColumnIfNotExists(Schema, (DB.MessageQueue x) => x.MessageInfo, x => x.AsString(int.MaxValue).Nullable());
            }
        }
    }
}
