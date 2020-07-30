using FluentMigrator;

namespace OnXap.Core.Db
{
    using DbSchema;

    class ModuleConfigSchemaItem : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            if (!Schema.Table<ModuleConfig>().Exists())
            {
                Create.Table<ModuleConfig>().
                    WithColumn((ModuleConfig x) => x.IdModule).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((ModuleConfig x) => x.UniqueKey).AsString(200).NotNullable().Unique($"t{GetTableName<ModuleConfig>()}_iUniqueKey").
                    WithColumn((ModuleConfig x) => x.Configuration).AsString(int.MaxValue).Nullable().
                    WithColumn((ModuleConfig x) => x.DateChange).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime).
                    WithColumn((ModuleConfig x) => x.IdUserChange).AsInt32().NotNullable().WithDefaultValue(0);
            }
            else
            {
                AddColumnIfNotExists(Schema, (ModuleConfig x) => x.IdModule, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (ModuleConfig x) => x.UniqueKey, x => x.AsString(200).NotNullable().Unique($"t{GetTableName<ModuleConfig>()}_iUniqueKey"));
                AddColumnIfNotExists(Schema, (ModuleConfig x) => x.Configuration, x => x.AsString(int.MaxValue).Nullable());
                AddColumnIfNotExists(Schema, (ModuleConfig x) => x.DateChange, x => x.AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime));
                AddColumnIfNotExists(Schema, (ModuleConfig x) => x.IdUserChange, x => x.AsInt32().NotNullable().WithDefaultValue(0));
            }
        }
    }
}
