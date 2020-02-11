using System.Linq;

namespace OnXap.Messaging.DB
{
    using Core.DbSchema;
    using Core.Db;

    class MessageSubscriptionRoleSchemaItem : DbSchemaItemRegular
    {
        public MessageSubscriptionRoleSchemaItem() : base(typeof(RoleSchemaItem), typeof(MessageSubscriptionRoleSchemaItem))
        {

        }

        public override void Down()
        {
        }

        public override void Up()
        {
            if (!Schema.Table<MessageSubscriptionRole>().Exists())
            {
                Create.Table<MessageSubscriptionRole>().
                    WithColumn((MessageSubscriptionRole x) => x.IdSubscription).AsInt32().NotNullable().
                    WithColumn((MessageSubscriptionRole x) => x.IdRole).AsInt32().NotNullable();

                //Create.PrimaryKey("MessageSubscriptionRoleKey").OnTable(FluentMigratorTableExtensions.GetTableName<MessageSubscriptionRole>()).Columns(
                //    FluentMigratorColumnExtensions.GetColumnName((MessageSubscriptionRole x) => x.IdSubscription),
                //    FluentMigratorColumnExtensions.GetColumnName((MessageSubscriptionRole x) => x.IdRole)
                //);

                //Create.ForeignKey("FK_MessageSubscriptionRole_MessageSubscription").
                //    FromTable(FluentMigratorTableExtensions.GetTableName<MessageSubscriptionRole>()).ForeignColumn(FluentMigratorColumnExtensions.GetColumnName((MessageSubscriptionRole x) => x.IdSubscription)).
                //    ToTable(FluentMigratorTableExtensions.GetTableName<MessageSubscription>()).PrimaryColumn(FluentMigratorColumnExtensions.GetColumnName((MessageSubscription x) => x.IdSubscription)).
                //    OnDelete(System.Data.Rule.Cascade);

                //Create.ForeignKey("FK_MessageSubscriptionRole_Role").
                //    FromTable(FluentMigratorTableExtensions.GetTableName<MessageSubscriptionRole>()).ForeignColumn(FluentMigratorColumnExtensions.GetColumnName((MessageSubscriptionRole x) => x.IdRole)).
                //    ToTable(FluentMigratorTableExtensions.GetTableName<Role>()).PrimaryColumn(FluentMigratorColumnExtensions.GetColumnName((Role x) => x.IdRole)).
                //    OnDelete(System.Data.Rule.Cascade);

            }
            else
            {
                AddColumnIfNotExists(Schema, (MessageSubscriptionRole x) => x.IdSubscription, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (MessageSubscriptionRole x) => x.IdRole, x => x.AsString(200).NotNullable());

                if (!Schema.Table<MessageSubscriptionRole>().Index("MessageSubscriptionRoleKey").Exists())
                    Create.PrimaryKey("MessageSubscriptionRoleKey").OnTable(FluentMigratorTableExtensions.GetTableName<MessageSubscriptionRole>()).Columns(
                        FluentMigratorColumnExtensions.GetColumnName((MessageSubscriptionRole x) => x.IdSubscription),
                        FluentMigratorColumnExtensions.GetColumnName((MessageSubscriptionRole x) => x.IdRole)
                    );

                if (!Schema.Table<MessageSubscriptionRole>().Constraint("FK_MessageSubscriptionRole_MessageSubscription").Exists())
                    Create.ForeignKey("FK_MessageSubscriptionRole_MessageSubscription").
                        FromTable(FluentMigratorTableExtensions.GetTableName<MessageSubscriptionRole>()).ForeignColumn(FluentMigratorColumnExtensions.GetColumnName((MessageSubscriptionRole x) => x.IdSubscription)).
                        ToTable(FluentMigratorTableExtensions.GetTableName<MessageSubscription>()).PrimaryColumn(FluentMigratorColumnExtensions.GetColumnName((MessageSubscription x) => x.IdSubscription)).
                        OnDelete(System.Data.Rule.Cascade);

                if (!Schema.Table<MessageSubscriptionRole>().Constraint("FK_MessageSubscriptionRole_Role").Exists())
                    Create.ForeignKey("FK_MessageSubscriptionRole_Role").
                        FromTable(FluentMigratorTableExtensions.GetTableName<MessageSubscriptionRole>()).ForeignColumn(FluentMigratorColumnExtensions.GetColumnName((MessageSubscriptionRole x) => x.IdRole)).
                        ToTable(FluentMigratorTableExtensions.GetTableName<Role>()).PrimaryColumn(FluentMigratorColumnExtensions.GetColumnName((Role x) => x.IdRole)).
                        OnDelete(System.Data.Rule.Cascade);
            }
        }
    }
}
