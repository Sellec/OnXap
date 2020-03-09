using FluentMigrator;

namespace OnXap.Core.Db
{
    using DbSchema;

    [Migration(202003100112, "Fix 'name' field")]
    class UserSchemaItem202003100112 : DbSchemaItem
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            if (Schema.Table<User>().Exists())
            {
                IfDatabase("sqlserver").Execute.Sql($"UPDATE [{GetTableName<User>()}] SET [{GetColumnName((User x) => x.name)}] = N'' WHERE [{GetColumnName((User x) => x.name)}] IS NULL");
                IfDatabase("sqlserver").Alter.Column(GetColumnName((User x) => x.name)).OnTable(GetTableName<User>()).AsString(200).NotNullable();
            }
        }
    }
}
