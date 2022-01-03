namespace OnXap.Modules.Subscriptions.DbSchema
{
    using Core.DbSchema;

    class Subscription : DbSchemaItemRegular
    {
        public Subscription() : base(typeof(SubscriptionGroup))
        {
        }

        public override void Down()
        {
        }

        public override void Up()
        {
            var isTableExists = Schema.Table<Db.Subscription>().Exists();
            var tableName = GetTableName<Db.Subscription>();

            if (!Schema.Table<Db.Subscription>().Exists())
            {
                Create.Table<Db.Subscription>().
                    WithColumn((Db.Subscription x) => x.IdSubscription).AsInt32().Identity().PrimaryKey().
                    WithColumn((Db.Subscription x) => x.NameSubscription).AsString(300).NotNullable().
                    WithColumn((Db.Subscription x) => x.IdGroup).AsInt32().NotNullable().
                    WithColumn((Db.Subscription x) => x.UniqueKey).AsGuid().NotNullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (Db.Subscription x) => x.IdSubscription, x => x.AsInt32().Identity().PrimaryKey());
                AddColumnIfNotExists(Schema, (Db.Subscription x) => x.NameSubscription, x => x.AsString(300).NotNullable());
                AddColumnIfNotExists(Schema, (Db.Subscription x) => x.IdGroup, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (Db.Subscription x) => x.UniqueKey, x => x.AsGuid().NotNullable());
            }

            if (!isTableExists || !Schema.Table<Db.Subscription>().Constraint("FK_Subscription_SubscriptionGroup").Exists())
                Create.ForeignKey("FK_Subscription_SubscriptionGroup").
                    FromTable(tableName).ForeignColumn(GetColumnName((Db.Subscription x) => x.IdGroup)).
                    ToTable(GetTableName<Db.SubscriptionGroup>()).PrimaryColumn(GetColumnName((Db.SubscriptionGroup x) => x.IdGroup)).
                    OnDelete(System.Data.Rule.Cascade);

            if (!isTableExists || !Schema.Table<Db.Subscription>().Index($"t{tableName}_UniqueKey").Exists())
                Create.Index($"t{tableName}_UniqueKey").OnTable(tableName).
                    OnColumn(GetColumnName((Db.Subscription x) => x.UniqueKey)).Ascending();//.
                    //WithOptions().UniqueNullsNotDistinct();
        }
    }
}