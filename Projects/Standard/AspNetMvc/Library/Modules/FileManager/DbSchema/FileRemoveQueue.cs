using FluentMigrator.SqlServer;

namespace OnXap.Modules.FileManager.DbSchema
{
    using Core.DbSchema;

    class FileRemoveQueue : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            if (!Schema.Table<Db.FileRemoveQueue>().Exists())
            {
                Create.Table<Db.FileRemoveQueue>().
                    WithColumn((Db.FileRemoveQueue x) => x.IdFile).AsInt32().NotNullable().PrimaryKey().Identity();
            }
            else
            {
                AddColumnIfNotExists(Schema, (Db.FileRemoveQueue x) => x.IdFile, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
            }
        }
    }
}
