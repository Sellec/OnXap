namespace OnXap.Messaging.DbSchema
{
    using Core.DbSchema;

    class MessagingContactData : DbSchemaItemRegular
    {
        public MessagingContactData() : base(typeof(MessagingContact), typeof(Core.Db.ItemTypeSchemaItem))
        {

        }

        public override void Down()
        {
        }

        public override void Up()
        {
            var isTableExists = Schema.Table<DB.MessagingContactData>().Exists();
            var tableName = GetTableName<DB.MessagingContactData>();

            if (!isTableExists)
            {
                Create.Table<DB.MessagingContactData>().
                    WithColumn((DB.MessagingContactData x) => x.IdMessagingContactData).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((DB.MessagingContactData x) => x.IdMessagingContact).AsInt32().NotNullable().
                    WithColumn((DB.MessagingContactData x) => x.IdMessagingServiceType).AsInt32().NotNullable().
                    WithColumn((DB.MessagingContactData x) => x.Data).AsString(500).NotNullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (DB.MessagingContactData x) => x.IdMessagingContactData, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (DB.MessagingContactData x) => x.IdMessagingContact, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.MessagingContactData x) => x.IdMessagingServiceType, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.MessagingContactData x) => x.Data, x => x.AsString(500).NotNullable());
            }

            if (!isTableExists || !Schema.Table<DB.MessagingContactData>().Constraint("FK_MessagingContactData_MessagingContact").Exists())
                Create.ForeignKey("FK_MessagingContactData_MessagingContact").
                    FromTable(tableName).ForeignColumn(GetColumnName((DB.MessagingContactData x) => x.IdMessagingContact)).
                    ToTable(GetTableName<DB.MessagingContact>()).PrimaryColumn(GetColumnName((DB.MessagingContact x) => x.IdMessagingContact)).
                    OnDelete(System.Data.Rule.Cascade);

            if (!isTableExists || !Schema.Table<DB.MessagingContactData>().Constraint("FK_MessagingContactData_ItemType").Exists())
                Create.ForeignKey("FK_MessagingContactData_ItemType").
                    FromTable(tableName).ForeignColumn(GetColumnName((DB.MessagingContactData x) => x.IdMessagingServiceType)).
                    ToTable(GetTableName<Core.Db.ItemType>()).PrimaryColumn(GetColumnName((Core.Db.ItemType x) => x.IdItemType)).
                    OnDelete(System.Data.Rule.Cascade);

            if (!isTableExists || !Schema.Table<DB.MessagingContactData>().Index("IX_MessagingContactData_IdMessagingContact").Exists())
                Create.Index("IX_MessagingContactData_IdMessagingContact").OnTable(tableName).
                    OnColumn(GetColumnName((DB.MessagingContactData x) => x.IdMessagingContact)).Ascending();
        }
    }
}
