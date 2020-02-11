namespace OnXap.Journaling.DB
{
    using Core.DbSchema;
    using Core.Db;

    class JournalDAOSchemaItem : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            /*
             * На этом объекте нельзя создавать внешние ключи, т.к. все запросы к объектам данного типа оборачиваются в Suppress-транзакцию.
             * */

            if (!Schema.Table<JournalDAO>().Exists())
            {
               
            }
            else
            {
                if (Schema.Table<JournalDAO>().Constraint("FK_Journal_UserBase").Exists())
                    Delete.ForeignKey("FK_Journal_UserBase").OnTable(FluentMigratorTableExtensions.GetTableName<JournalDAO>());
            }
        }
    }
}
