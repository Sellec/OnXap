using FluentMigrator.SqlServer;

namespace OnXap.Modules.Materials.DbSchema
{
    using Core.DbSchema;
    using Core.Items.Db;

    class News : DbSchemaItemRegular
    {
        public News() : base()
        {

        }

        public override void Down()
        {
        }

        public override void Up()
        {
            /*
             * На этом объекте нельзя создавать внешние ключи к таблице User, т.к. все запросы к объектам данного типа оборачиваются в Suppress-транзакцию.
             * */
            var isTableExists = Schema.Table<DB.News>().Exists();
            var tableName = GetTableName<DB.News>();

            if (!isTableExists)
            {
                Create.Table<DB.News>().
                    WithColumn((DB.News x) => x.id).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((DB.News x) => x.category).AsInt32().NotNullable().
                    WithColumn((DB.News x) => x.status).AsBoolean().NotNullable().
                    WithColumn((DB.News x) => x.name).AsString(300).NotNullable().
                    WithColumn((DB.News x) => x.short_text).AsString(int.MaxValue).Nullable().
                    WithColumn((DB.News x) => x.text).AsString(int.MaxValue).Nullable().
                    WithColumn((DB.News x) => x.date).AsDateTime().NotNullable().
                    WithColumn((DB.News x) => x.comments_count).AsInt32().NotNullable().
                    WithColumn((DB.News x) => x.user).AsInt32().NotNullable().
                    WithColumn((DB.News x) => x.Block).AsBoolean().NotNullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (DB.News x) => x.id, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (DB.News x) => x.category, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.News x) => x.status, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.News x) => x.name, x => x.AsString(300).NotNullable());
                AddColumnIfNotExists(Schema, (DB.News x) => x.short_text, x => x.AsString(int.MaxValue).Nullable());
                AddColumnIfNotExists(Schema, (DB.News x) => x.text, x => x.AsString(int.MaxValue).Nullable());
                AddColumnIfNotExists(Schema, (DB.News x) => x.date, x => x.AsDateTime().NotNullable());
                AddColumnIfNotExists(Schema, (DB.News x) => x.comments_count, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.News x) => x.user, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.News x) => x.Block, x => x.AsInt32().NotNullable());
            }
        }
    }
}
