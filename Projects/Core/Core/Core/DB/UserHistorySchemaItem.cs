using FluentMigrator;

namespace OnXap.Core.Db
{
    using DbSchema;

    class UserHistorySchemaItem : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            if (!Schema.Table<UserHistory>().Exists())
            {
                Create.Table<UserHistory>().
                    WithColumn((UserHistory x) => x.IdUserHistory).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((UserHistory x) => x.DateChangeHistory).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime).
                    WithColumn((UserHistory x) => x.IdUser).AsInt32().NotNullable().
                    WithColumn((UserHistory x) => x.UniqueKey).AsString(200).Nullable().
                    WithColumn((UserHistory x) => x.email).AsString(128).Nullable().
                    WithColumn((UserHistory x) => x.phone).AsString(100).Nullable().
                    WithColumn((UserHistory x) => x.password).AsString(64).Nullable().
                    WithColumn((UserHistory x) => x.salt).AsString(5).Nullable().
                    WithColumn((UserHistory x) => x.name).AsString(200).Nullable().
                    WithColumn((UserHistory x) => x.IdPhoto).AsInt32().Nullable().
                    WithColumn((UserHistory x) => x.Superuser).AsByte().NotNullable().WithDefaultValue(0).
                    WithColumn((UserHistory x) => x.State).AsInt16().NotNullable().WithDefaultValue(0).
                    WithColumn((UserHistory x) => x.StateConfirmation).AsString(100).Nullable().
                    WithColumn((UserHistory x) => x.AuthorizationAttempts).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((UserHistory x) => x.Block).AsInt16().NotNullable().WithDefaultValue(0).
                    WithColumn((UserHistory x) => x.BlockedUntil).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((UserHistory x) => x.BlockedReason).AsString(500).Nullable().
                    WithColumn((UserHistory x) => x.DateChange).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((UserHistory x) => x.IdUserChange).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((UserHistory x) => x.Comment).AsString(int.MaxValue).Nullable().
                    WithColumn((UserHistory x) => x.CommentAdmin).AsString(int.MaxValue).Nullable().
                    WithColumn((UserHistory x) => x.about).AsString(int.MaxValue).Nullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (UserHistory x) => x.IdUserHistory, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (UserHistory x) => x.DateChangeHistory, x => x.AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime));
                AddColumnIfNotExists(Schema, (UserHistory x) => x.IdUser, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (UserHistory x) => x.UniqueKey, x => x.AsString(200).Nullable());
                AddColumnIfNotExists(Schema, (UserHistory x) => x.email, x => x.AsString(128).Nullable());
                AddColumnIfNotExists(Schema, (UserHistory x) => x.phone, x => x.AsString(100).Nullable());
                AddColumnIfNotExists(Schema, (UserHistory x) => x.password, x => x.AsString(64).Nullable());
                AddColumnIfNotExists(Schema, (UserHistory x) => x.salt, x => x.AsString(5).Nullable());
                AddColumnIfNotExists(Schema, (UserHistory x) => x.name, x => x.AsString(200).Nullable());
                AddColumnIfNotExists(Schema, (UserHistory x) => x.IdPhoto, x => x.AsInt32().Nullable());
                AddColumnIfNotExists(Schema, (UserHistory x) => x.Superuser, x => x.AsByte().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (UserHistory x) => x.State, x => x.AsInt16().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (UserHistory x) => x.StateConfirmation, x => x.AsString(100).Nullable());
                AddColumnIfNotExists(Schema, (UserHistory x) => x.AuthorizationAttempts, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (UserHistory x) => x.Block, x => x.AsInt16().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (UserHistory x) => x.BlockedUntil, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (UserHistory x) => x.BlockedReason, x => x.AsString(500).Nullable());
                AddColumnIfNotExists(Schema, (UserHistory x) => x.DateChange, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (UserHistory x) => x.IdUserChange, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (UserHistory x) => x.Comment, x => x.AsString(int.MaxValue).Nullable());
                AddColumnIfNotExists(Schema, (UserHistory x) => x.CommentAdmin, x => x.AsString(int.MaxValue).Nullable());
                AddColumnIfNotExists(Schema, (UserHistory x) => x.about, x => x.AsString(int.MaxValue).Nullable());
            }
        }
    }
}
