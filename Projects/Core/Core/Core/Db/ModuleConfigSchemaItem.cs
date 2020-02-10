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
            if (!Schema.Table("ModuleConfig").Exists())
            {
                Create.Table("ModuleConfig").
                    WithColumn("IdModule").AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn("UniqueKey").AsString(200).NotNullable().Unique("UniqueKey").
                    WithColumn("Configuration").AsString(int.MaxValue).Nullable().
                    WithColumn("DateChange").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime).
                    WithColumn("IdUserChange").AsInt32().NotNullable().WithDefaultValue(0);
            }
            else
            {
                if (!Schema.Table("ModuleConfig").Column("IdModule").Exists())
                    Alter.Table("ModuleConfig").AddColumn("IdModule").AsInt32().NotNullable().PrimaryKey().Identity();

                if (!Schema.Table("ModuleConfig").Column("UniqueKey").Exists())
                    Alter.Table("ModuleConfig").AddColumn("UniqueKey").AsString(200).NotNullable().Unique("UniqueKey");

                if (!Schema.Table("ModuleConfig").Column("Configuration").Exists())
                    Alter.Table("ModuleConfig").AddColumn("Configuration").AsString(int.MaxValue).Nullable();

                if (!Schema.Table("ModuleConfig").Column("DateChange").Exists())
                    Alter.Table("ModuleConfig").AddColumn("DateChange").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

                if (!Schema.Table("ModuleConfig").Column("IdUserChange").Exists())
                    Alter.Table("ModuleConfig").AddColumn("IdUserChange").AsInt32().NotNullable().WithDefaultValue(0);

            }
        }
    }
}
