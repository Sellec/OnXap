namespace OnXap.Users.Db
{
    using Core.DbSchema;

    class UserEntitySchemaItem : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            if (!Schema.Table<UserEntity>().Exists())
            {
                Create.Table<UserEntity>().
                    WithColumn((UserEntity x) => x.IdEntity).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((UserEntity x) => x.IdUser).AsInt32().NotNullable().
                    WithColumn((UserEntity x) => x.Tag).AsString(200).NotNullable().
                    WithColumn((UserEntity x) => x.EntityType).AsString(200).NotNullable().
                    WithColumn((UserEntity x) => x.Entity).AsString(int.MaxValue).NotNullable().
                    WithColumn((UserEntity x) => x.IsTagged).AsBoolean().NotNullable().
                    WithColumn((UserEntity x) => x.UniqueKey).AsString(250).Nullable();

                IfDatabase("sqlserver").Execute.Sql($"CREATE UNIQUE NONCLUSTERED INDEX [UniqueKey] ON [{FluentMigratorTableExtensions.GetTableName<UserEntity>()}] ([{FluentMigratorColumnExtensions.GetColumnName((UserEntity x) => x.UniqueKey)}] ASC) WHERE ([{FluentMigratorColumnExtensions.GetColumnName((UserEntity x) => x.UniqueKey)}] IS NOT NULL);");
            }
            else
            {
                AddColumnIfNotExists(Schema, (UserEntity x) => x.IdEntity, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (UserEntity x) => x.IdUser, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (UserEntity x) => x.Tag, x => x.AsString(200).NotNullable());
                AddColumnIfNotExists(Schema, (UserEntity x) => x.EntityType, x => x.AsString(200).NotNullable());
                AddColumnIfNotExists(Schema, (UserEntity x) => x.Entity, x => x.AsString(int.MaxValue).NotNullable());
                AddColumnIfNotExists(Schema, (UserEntity x) => x.IsTagged, x => x.AsBoolean().NotNullable());
                AddColumnIfNotExists(Schema, (UserEntity x) => x.UniqueKey, x => x.AsString(250).Nullable());

                if (!Schema.Table<UserEntity>().Index("UniqueKey").Exists())
                    IfDatabase("sqlserver").Execute.Sql($"CREATE UNIQUE NONCLUSTERED INDEX [UniqueKey] ON [{FluentMigratorTableExtensions.GetTableName<UserEntity>()}] ([{FluentMigratorColumnExtensions.GetColumnName((UserEntity x) => x.UniqueKey)}] ASC) WHERE ([{FluentMigratorColumnExtensions.GetColumnName((UserEntity x) => x.UniqueKey)}] IS NOT NULL);");
            }
        }
    }
}
