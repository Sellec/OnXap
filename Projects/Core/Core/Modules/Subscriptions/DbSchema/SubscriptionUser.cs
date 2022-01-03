namespace OnXap.Modules.Subscriptions.DbSchema
{
    using Core.DbSchema;

    class SubscriptionUser : DbSchemaItemRegular
    {
        public SubscriptionUser() : base(typeof(Subscription))
        {
        }

        public override void Down()
        {
        }

        public override void Up()
        {
            var isTableExists = Schema.Table<Db.SubscriptionUser>().Exists();
            var tableName = GetTableName<Db.SubscriptionUser>();

            if (!Schema.Table<Db.SubscriptionUser>().Exists())
            {
                Create.Table<Db.SubscriptionUser>().
                    WithColumn((Db.SubscriptionUser x) => x.IdSubscription).AsInt32().NotNullable().PrimaryKey().
                    WithColumn((Db.SubscriptionUser x) => x.IdUser).AsInt32().NotNullable().PrimaryKey();
            }
            else
            {
                AddColumnIfNotExists(Schema, (Db.SubscriptionUser x) => x.IdSubscription, x => x.AsInt32().NotNullable().PrimaryKey());
                AddColumnIfNotExists(Schema, (Db.SubscriptionUser x) => x.IdUser, x => x.AsInt32().NotNullable().PrimaryKey());
            }

            if (!isTableExists || !Schema.Table<Db.SubscriptionUser>().Constraint("FK_SubscriptionUser_Subscription").Exists())
                Create.ForeignKey("FK_SubscriptionUser_Subscription").
                    FromTable(tableName).ForeignColumn(GetColumnName((Db.SubscriptionUser x) => x.IdSubscription)).
                    ToTable(GetTableName<Db.Subscription>()).PrimaryColumn(GetColumnName((Db.Subscription x) => x.IdSubscription)).
                    OnDelete(System.Data.Rule.Cascade);

            if (!isTableExists || !Schema.Table<Db.SubscriptionUser>().Constraint("FK_SubscriptionUser_User").Exists())
                Create.ForeignKey("FK_SubscriptionUser_User").
                    FromTable(tableName).ForeignColumn(GetColumnName((Db.SubscriptionUser x) => x.IdSubscription)).
                    ToTable(GetTableName<Core.Db.User>()).PrimaryColumn(GetColumnName((Core.Db.User x) => x.IdUser)).
                    OnDelete(System.Data.Rule.Cascade);

        }
    }
}