using FluentMigrator.SqlServer;

namespace OnXap.Modules.Materials.DbSchema
{
    using Core.DbSchema;
    using Core.Items.Db;

    class Page : DbSchemaItemRegular
    {
        public Page() : base()
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
            var isTableExists = Schema.Table<DB.Page>().Exists();
            var tableName = GetTableName<DB.Page>();

            if (!isTableExists)
            {
                Create.Table<DB.Page>().
                    WithColumn((DB.Page x) => x.id).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((DB.Page x) => x.category).AsInt32().Nullable().
                    WithColumn((DB.Page x) => x.subs_id).AsString(int.MaxValue).NotNullable().
                    WithColumn((DB.Page x) => x.subs_order).AsString(int.MaxValue).NotNullable().
                    WithColumn((DB.Page x) => x.status).AsByte().NotNullable().
                    WithColumn((DB.Page x) => x.language).AsString(20).NotNullable().
                    WithColumn((DB.Page x) => x.name).AsString(int.MaxValue).NotNullable().
                    WithColumn((DB.Page x) => x.urlname).AsString(int.MaxValue).NotNullable().
                    WithColumn((DB.Page x) => x.body).AsString(int.MaxValue).NotNullable().
                    WithColumn((DB.Page x) => x.parent).AsByte().NotNullable().
                    WithColumn((DB.Page x) => x.order).AsInt32().NotNullable().
                    WithColumn((DB.Page x) => x.photo).AsString(int.MaxValue).NotNullable().
                    WithColumn((DB.Page x) => x.count_views).AsInt32().NotNullable().
                    WithColumn((DB.Page x) => x.comments_count).AsInt32().NotNullable().
                    WithColumn((DB.Page x) => x.pages_gallery).AsInt32().NotNullable().
                    WithColumn((DB.Page x) => x.news_id).AsInt32().NotNullable().
                    WithColumn((DB.Page x) => x.seo_title).AsString(255).NotNullable().
                    WithColumn((DB.Page x) => x.seo_descr).AsString(int.MaxValue).NotNullable().
                    WithColumn((DB.Page x) => x.seo_kw).AsString(int.MaxValue).NotNullable().
                    WithColumn((DB.Page x) => x.ajax_name).AsString(255).NotNullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (DB.Page x) => x.id, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.category, x => x.AsInt32().Nullable());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.subs_id, x =>x.AsString(int.MaxValue).NotNullable());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.subs_order, x => x.AsString(int.MaxValue).NotNullable());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.status, x => x.AsByte().NotNullable());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.language, x => x.AsString(20).NotNullable());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.name, x => x.AsString(int.MaxValue).NotNullable());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.urlname, x => x.AsString(int.MaxValue).NotNullable());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.body, x => x.AsString(int.MaxValue).NotNullable());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.parent, x => x.AsByte().NotNullable());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.order, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.photo, x => x.AsString(int.MaxValue).NotNullable());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.count_views, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.comments_count, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.pages_gallery, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.news_id, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.seo_title, x => x.AsString(255).NotNullable());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.seo_descr, x => x.AsString(int.MaxValue).NotNullable());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.seo_kw, x => x.AsString(int.MaxValue).NotNullable());
                AddColumnIfNotExists(Schema, (DB.Page x) => x.ajax_name, x => x.AsString(255).NotNullable());
            }
        }
    }
}
