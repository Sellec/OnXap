namespace OnXap.Modules.Subscriptions.DbSchema
{
    using Core.DbSchema;

    class SubscriptionGroup : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            var isTableExists = Schema.Table<Db.SubscriptionGroup>().Exists();

            if (!Schema.Table<Db.SubscriptionGroup>().Exists())
            {
                Create.Table<Db.SubscriptionGroup>().
                    WithColumn((Db.SubscriptionGroup x) => x.IdGroup).AsInt32().Identity().PrimaryKey().
                    WithColumn((Db.SubscriptionGroup x) => x.NameGroup).AsString(300).NotNullable().
                    WithColumn((Db.SubscriptionGroup x) => x.UniqueKey).AsGuid().NotNullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (Db.SubscriptionGroup x) => x.IdGroup, x => x.AsInt32().Identity().PrimaryKey());
                AddColumnIfNotExists(Schema, (Db.SubscriptionGroup x) => x.NameGroup, x => x.AsString(300).NotNullable());
                AddColumnIfNotExists(Schema, (Db.SubscriptionGroup x) => x.UniqueKey, x => x.AsGuid().NotNullable());
            }

            if (!isTableExists || !Schema.Table<Db.SubscriptionGroup>().Index($"t{GetTableName<Db.SubscriptionGroup>()}_UniqueKey").Exists())
                Create.Index($"t{GetTableName<Db.SubscriptionGroup>()}_UniqueKey").OnTable(GetTableName<Db.SubscriptionGroup>()).
                    OnColumn(GetColumnName((Db.SubscriptionGroup x) => x.UniqueKey)).Ascending();//.
                    //WithOptions().UniqueNullsNotDistinct();
        }
    }
}