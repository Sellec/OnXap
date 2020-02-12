namespace OnXap.Modules.Auth.Db
{
    using Core.DbSchema;

    class UserPasswordRecoverySchemaItem : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            if (!Schema.Table<UserPasswordRecovery>().Exists())
            {
                Create.Table<UserPasswordRecovery>().
                    WithColumn((UserPasswordRecovery x) => x.IdRecovery).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((UserPasswordRecovery x) => x.IdUser).AsInt32().NotNullable().
                    WithColumn((UserPasswordRecovery x) => x.RecoveryKey).AsString(32).NotNullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (UserPasswordRecovery x) => x.IdRecovery, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (UserPasswordRecovery x) => x.IdUser, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (UserPasswordRecovery x) => x.RecoveryKey, x => x.AsString(32).NotNullable());
            }
        }
    }
}
