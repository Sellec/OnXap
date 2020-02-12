namespace OnXap.Core.Db
{
    using DbSchema;

    class UserSessionSchemaItem : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            if (!Schema.Table<UserSession>().Exists())
            {
                Create.Table<UserSession>().
                    WithColumn((UserSession x) => x.SessionId).AsAnsiString(24).NotNullable().PrimaryKey().
                    WithColumn((UserSession x) => x.Created).AsDateTime().NotNullable().
                    WithColumn((UserSession x) => x.Expires).AsDateTime().NotNullable().
                    WithColumn((UserSession x) => x.LockDate).AsDateTime().NotNullable().
                    WithColumn((UserSession x) => x.LockId).AsInt32().NotNullable().
                    WithColumn((UserSession x) => x.Locked).AsBoolean().NotNullable().
                    WithColumn((UserSession x) => x.ItemContent).AsBinary(int.MaxValue).Nullable().
                    WithColumn((UserSession x) => x.IdUser).AsInt32().NotNullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (UserSession x) => x.SessionId, x => x.AsAnsiString(24).NotNullable().PrimaryKey());
                AddColumnIfNotExists(Schema, (UserSession x) => x.Created, x => x.AsDateTime().NotNullable());
                AddColumnIfNotExists(Schema, (UserSession x) => x.Expires, x => x.AsDateTime().NotNullable());
                AddColumnIfNotExists(Schema, (UserSession x) => x.LockDate, x => x.AsDateTime().NotNullable());
                AddColumnIfNotExists(Schema, (UserSession x) => x.LockId, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (UserSession x) => x.Locked, x => x.AsBoolean().NotNullable());
                AddColumnIfNotExists(Schema, (UserSession x) => x.ItemContent, x => x.AsBinary(int.MaxValue).Nullable());
                AddColumnIfNotExists(Schema, (UserSession x) => x.IdUser, x => x.AsInt32().NotNullable());

            }
        }
    }
}
