namespace OnXap.Messaging.DbSchema
{
    using Core.DbSchema;

    class MessagingContact : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            var isTableExists = Schema.Table<DB.MessagingContact>().Exists();

            if (!isTableExists)
            {
                Create.Table<DB.MessagingContact>().
                    WithColumn((DB.MessagingContact x) => x.IdMessagingContact).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((DB.MessagingContact x) => x.NameFull).AsString(500).NotNullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (DB.MessagingContact x) => x.IdMessagingContact, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (DB.MessagingContact x) => x.NameFull, x => x.AsString(500).NotNullable());
            }
        }
    }
}
