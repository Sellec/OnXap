namespace OnXap.Core.Db
{
    using DbSchema;

    class RoleUserSchemaItem : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            if (!Schema.Table<RoleUser>().Exists())
            {
                Create.Table<RoleUser>().
                    WithColumn((RoleUser x) => x.IdRole).AsInt32().NotNullable().
                    WithColumn((RoleUser x) => x.IdUser).AsInt32().NotNullable().
                    WithColumn((RoleUser x) => x.IdUserChange).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((RoleUser x) => x.DateChange).AsInt32().NotNullable().WithDefaultValue(0);

                Create.PrimaryKey("RoleUserKey").OnTable(FluentMigratorTableExtensions.GetTableName<RoleUser>()).Columns(
                    FluentMigratorColumnExtensions.GetColumnName((RoleUser x) => x.IdRole),
                    FluentMigratorColumnExtensions.GetColumnName((RoleUser x) => x.IdUser)
                );
            }
            else
            {
                AddColumnIfNotExists(Schema, (RoleUser x) => x.IdRole, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (RoleUser x) => x.IdUser, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (RoleUser x) => x.IdUserChange, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (RoleUser x) => x.DateChange, x => x.AsInt32().NotNullable().WithDefaultValue(0));

                if (!Schema.Table<RoleUser>().Index("RoleUserKey").Exists())
                    Create.PrimaryKey("RoleUserKey").OnTable(FluentMigratorTableExtensions.GetTableName<RoleUser>()).Columns(
                        FluentMigratorColumnExtensions.GetColumnName((RoleUser x) => x.IdRole),
                        FluentMigratorColumnExtensions.GetColumnName((RoleUser x) => x.IdUser)
                    );
            }
        }
    }
}
