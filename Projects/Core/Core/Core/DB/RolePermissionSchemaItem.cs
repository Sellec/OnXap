namespace OnXap.Core.Db
{
    using DbSchema;

    class RolePermissionSchemaItem : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            if (!Schema.Table<RolePermission>().Exists())
            {
                Create.Table<RolePermission>().
                    WithColumn((RolePermission x) => x.IdRole).AsInt32().NotNullable().
                    WithColumn((RolePermission x) => x.IdModule).AsInt32().NotNullable().
                    WithColumn((RolePermission x) => x.Permission).AsString(200).NotNullable().
                    WithColumn((RolePermission x) => x.IdUserChange).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((RolePermission x) => x.DateChange).AsInt32().NotNullable().WithDefaultValue(0);

                Create.PrimaryKey("RolePermissionKey").OnTable(FluentMigratorTableExtensions.GetTableName<RolePermission>()).Columns(
                    FluentMigratorColumnExtensions.GetColumnName((RolePermission x) => x.IdRole),
                    FluentMigratorColumnExtensions.GetColumnName((RolePermission x) => x.IdModule),
                    FluentMigratorColumnExtensions.GetColumnName((RolePermission x) => x.Permission)
                );
            }
            else
            {
                AddColumnIfNotExists(Schema, (RolePermission x) => x.IdRole, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (RolePermission x) => x.IdModule, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (RolePermission x) => x.Permission, x => x.AsString(200).NotNullable());
                AddColumnIfNotExists(Schema, (RolePermission x) => x.IdUserChange, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (RolePermission x) => x.DateChange, x => x.AsInt32().NotNullable().WithDefaultValue(0));

                if (!Schema.Table<RolePermission>().Index("RolePermissionKey").Exists())
                    Create.PrimaryKey("RolePermissionKey").OnTable(FluentMigratorTableExtensions.GetTableName<RolePermission>()).Columns(
                        FluentMigratorColumnExtensions.GetColumnName((RolePermission x) => x.IdRole),
                        FluentMigratorColumnExtensions.GetColumnName((RolePermission x) => x.IdModule),
                        FluentMigratorColumnExtensions.GetColumnName((RolePermission x) => x.Permission)
                    );
            }
        }
    }
}
