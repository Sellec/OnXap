﻿using FluentMigrator.SqlServer;

namespace OnXap.Journaling.DB
{
    using Core.DbSchema;
    using Core.Items.Db;

    class JournalSchemaItem : DbSchemaItemRegular
    {
        public JournalSchemaItem() : base(typeof(ItemLinkSchemaItem))
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
            var isTableExists = Schema.Table<JournalDAO>().Exists();
            var tableName = GetTableName<JournalDAO>();

            if (!isTableExists)
            {
                Create.Table<JournalDAO>().
                    WithColumn((JournalDAO x) => x.IdJournalData).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((JournalDAO x) => x.IdJournal).AsInt32().NotNullable().
                    WithColumn((JournalDAO x) => x.EventType).AsByte().NotNullable().WithDefaultValue((byte)EventType.Info).
                    WithColumn((JournalDAO x) => x.EventInfo).AsString(300).NotNullable().
                    WithColumn((JournalDAO x) => x.EventInfoDetailed).AsString(int.MaxValue).Nullable().
                    WithColumn((JournalDAO x) => x.EventCode).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((JournalDAO x) => x.ExceptionDetailed).AsString(int.MaxValue).Nullable().
                    WithColumn((JournalDAO x) => x.DateEvent).AsDateTime().NotNullable().
                    WithColumn((JournalDAO x) => x.IdUser).AsInt32().Nullable().
                    WithColumn((JournalDAO x) => x.ItemLinkId).AsGuid().Nullable();

            }
            else
            {
                AddColumnIfNotExists(Schema, (JournalDAO x) => x.IdJournalData, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (JournalDAO x) => x.IdJournal, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (JournalDAO x) => x.EventType, x => x.AsByte().NotNullable().WithDefaultValue((byte)EventType.Info));
                AddColumnIfNotExists(Schema, (JournalDAO x) => x.EventInfo, x => x.AsString(300).NotNullable());
                AddColumnIfNotExists(Schema, (JournalDAO x) => x.EventInfoDetailed, x => x.AsString(int.MaxValue).Nullable());
                AddColumnIfNotExists(Schema, (JournalDAO x) => x.EventCode, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (JournalDAO x) => x.ExceptionDetailed, x => x.AsString(int.MaxValue).Nullable());
                AddColumnIfNotExists(Schema, (JournalDAO x) => x.DateEvent, x => x.AsDateTime().NotNullable());
                AddColumnIfNotExists(Schema, (JournalDAO x) => x.IdUser, x => x.AsInt32().Nullable());
                AddColumnIfNotExists(Schema, (JournalDAO x) => x.ItemLinkId, x => x.AsGuid().Nullable());

                if (Schema.Table<JournalDAO>().Constraint("FK_Journal_UserBase").Exists())
                    Delete.ForeignKey("FK_Journal_UserBase").OnTable(GetTableName<JournalDAO>());
            }

            if (!isTableExists || !Schema.Table<JournalDAO>().Index("IX_ItemLinkId").Exists())
                Create.Index("IX_ItemLinkId").OnTable(GetTableName<JournalDAO>()).
                    OnColumn(GetColumnName((JournalDAO x) => x.ItemLinkId)).Ascending();

            if (!isTableExists || !Schema.Table<JournalDAO>().Constraint("FK_Journal_ItemLink").Exists())
                Create.ForeignKey("FK_Journal_ItemLink").
                    FromTable(GetTableName<JournalDAO>()).ForeignColumn(GetColumnName((JournalDAO x) => x.ItemLinkId)).
                    ToTable(GetTableName<ItemLink>()).PrimaryColumn(GetColumnName((ItemLink x) => x.LinkId)).
                    OnDelete(System.Data.Rule.Cascade);

            if (!isTableExists || !Schema.Table<JournalDAO>().Index("Journal_IdJournal_EventCode_with_IdJournalData").Exists())
                Create.Index("Journal_IdJournal_EventCode_with_IdJournalData").OnTable(GetTableName<JournalDAO>()).
                    OnColumn(GetColumnName((JournalDAO x) => x.IdJournal)).Descending().
                    OnColumn(GetColumnName((JournalDAO x) => x.EventCode)).Ascending().
                    Include(GetColumnName((JournalDAO x) => x.IdJournalData));


        }
    }
}
