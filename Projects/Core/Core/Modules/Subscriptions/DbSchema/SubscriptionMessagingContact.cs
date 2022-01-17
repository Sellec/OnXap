namespace OnXap.Modules.Subscriptions.DbSchema
{
    using Core.DbSchema;

    class SubscriptionMessagingContact : DbSchemaItemRegular
    {
        public SubscriptionMessagingContact() : base(typeof(Subscription), typeof(Messaging.DbSchema.MessagingContact))
        {
        }

        public override void Down()
        {
        }

        public override void Up()
        {
            var isTableExists = Schema.Table<Db.SubscriptionMessagingContact>().Exists();
            var tableName = GetTableName<Db.SubscriptionMessagingContact>();

            if (!Schema.Table<Db.SubscriptionMessagingContact>().Exists())
            {
                Create.Table<Db.SubscriptionMessagingContact>().
                    WithColumn((Db.SubscriptionMessagingContact x) => x.IdSubscription).AsInt32().NotNullable().PrimaryKey().
                    WithColumn((Db.SubscriptionMessagingContact x) => x.IdMessagingContact).AsInt32().NotNullable().PrimaryKey();
            }
            else
            {
                AddColumnIfNotExists(Schema, (Db.SubscriptionMessagingContact x) => x.IdSubscription, x => x.AsInt32().NotNullable().PrimaryKey());
                AddColumnIfNotExists(Schema, (Db.SubscriptionMessagingContact x) => x.IdMessagingContact, x => x.AsInt32().NotNullable().PrimaryKey());
            }

            if (!isTableExists || !Schema.Table<Db.SubscriptionMessagingContact>().Constraint("FK_SubscriptionMessagingContact_Subscription").Exists())
                Create.ForeignKey("FK_SubscriptionMessagingContact_Subscription").
                    FromTable(tableName).ForeignColumn(GetColumnName((Db.SubscriptionMessagingContact x) => x.IdSubscription)).
                    ToTable(GetTableName<Db.Subscription>()).PrimaryColumn(GetColumnName((Db.Subscription x) => x.IdSubscription)).
                    OnDelete(System.Data.Rule.Cascade);

            if (!isTableExists || !Schema.Table<Db.SubscriptionMessagingContact>().Constraint("FK_SubscriptionMessagingContact_MessagingContact").Exists())
                Create.ForeignKey("FK_SubscriptionMessagingContact_MessagingContact").
                    FromTable(tableName).ForeignColumn(GetColumnName((Db.SubscriptionMessagingContact x) => x.IdMessagingContact)).
                    ToTable(GetTableName<Messaging.DB.MessagingContact>()).PrimaryColumn(GetColumnName((Messaging.DB.MessagingContact x) => x.IdMessagingContact)).
                    OnDelete(System.Data.Rule.Cascade);

        }
    }
}