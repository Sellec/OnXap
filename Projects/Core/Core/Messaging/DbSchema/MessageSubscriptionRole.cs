using System.Linq;

namespace OnXap.Messaging.DbSchema
{
    using Core.DbSchema;
    using Core.Db;

    class MessageSubscriptionRole : DbSchemaItemRegular
    {
        public MessageSubscriptionRole() : base(typeof(RoleSchemaItem), typeof(MessageSubscription))
        {

        }

        public override void Down()
        {
        }

        public override void Up()
        {
            if (!Schema.Table<DB.MessageSubscriptionRole>().Exists())
            {
                Create.Table<DB.MessageSubscriptionRole>().
                    WithColumn((DB.MessageSubscriptionRole x) => x.IdSubscription).AsInt32().NotNullable().
                    WithColumn((DB.MessageSubscriptionRole x) => x.IdRole).AsInt32().NotNullable();

                Create.PrimaryKey("DB.MessageSubscriptionRoleKey").OnTable(GetTableName<DB.MessageSubscriptionRole>()).Columns(
                    GetColumnName((DB.MessageSubscriptionRole x) => x.IdSubscription),
                    GetColumnName((DB.MessageSubscriptionRole x) => x.IdRole)
                );

                Create.ForeignKey("FK_DB.MessageSubscriptionRole_MessageSubscription").
                    FromTable(GetTableName<DB.MessageSubscriptionRole>()).ForeignColumn(GetColumnName((DB.MessageSubscriptionRole x) => x.IdSubscription)).
                    ToTable(GetTableName<MessageSubscription>()).PrimaryColumn(GetColumnName((DB.MessageSubscription x) => x.IdSubscription)).
                    OnDelete(System.Data.Rule.Cascade);

                Create.ForeignKey("FK_DB.MessageSubscriptionRole_Role").
                    FromTable(GetTableName<DB.MessageSubscriptionRole>()).ForeignColumn(GetColumnName((DB.MessageSubscriptionRole x) => x.IdRole)).
                    ToTable(GetTableName<Role>()).PrimaryColumn(GetColumnName((Role x) => x.IdRole)).
                    OnDelete(System.Data.Rule.Cascade);

            }
            else
            {
                AddColumnIfNotExists(Schema, (DB.MessageSubscriptionRole x) => x.IdSubscription, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (DB.MessageSubscriptionRole x) => x.IdRole, x => x.AsString(200).NotNullable());

                if (!Schema.Table<DB.MessageSubscriptionRole>().Index("DB.MessageSubscriptionRoleKey").Exists())
                    Create.PrimaryKey("DB.MessageSubscriptionRoleKey").OnTable(GetTableName<DB.MessageSubscriptionRole>()).Columns(
                        GetColumnName((DB.MessageSubscriptionRole x) => x.IdSubscription),
                        GetColumnName((DB.MessageSubscriptionRole x) => x.IdRole)
                    );

                if (!Schema.Table<DB.MessageSubscriptionRole>().Constraint("FK_DB.MessageSubscriptionRole_MessageSubscription").Exists())
                    Create.ForeignKey("FK_DB.MessageSubscriptionRole_MessageSubscription").
                        FromTable(GetTableName<DB.MessageSubscriptionRole>()).ForeignColumn(GetColumnName((DB.MessageSubscriptionRole x) => x.IdSubscription)).
                        ToTable(GetTableName<MessageSubscription>()).PrimaryColumn(GetColumnName((DB.MessageSubscription x) => x.IdSubscription)).
                        OnDelete(System.Data.Rule.Cascade);

                if (!Schema.Table<DB.MessageSubscriptionRole>().Constraint("FK_DB.MessageSubscriptionRole_Role").Exists())
                    Create.ForeignKey("FK_DB.MessageSubscriptionRole_Role").
                        FromTable(GetTableName<DB.MessageSubscriptionRole>()).ForeignColumn(GetColumnName((DB.MessageSubscriptionRole x) => x.IdRole)).
                        ToTable(GetTableName<Role>()).PrimaryColumn(GetColumnName((Role x) => x.IdRole)).
                        OnDelete(System.Data.Rule.Cascade);
            }
        }
    }
}
