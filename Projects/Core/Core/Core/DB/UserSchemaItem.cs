using FluentMigrator.SqlServer;
using System.Linq;

namespace OnXap.Core.Db
{
    using DbSchema;

    class UserSchemaItem : DbSchemaItemRegular
    {
        public UserSchemaItem() : base(typeof(UserHistorySchemaItem))
        {

        }

        public override void Down()
        {
        }

        public override void Up()
        {
            if (!Schema.Table<User>().Exists())
            {
                Create.Table<User>().
                    WithColumn((User x) => x.IdUser).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((User x) => x.UniqueKey).AsString(200).Nullable().
                    WithColumn((User x) => x.email).AsString(128).Nullable().
                    WithColumn((User x) => x.phone).AsString(100).Nullable().
                    WithColumn((User x) => x.password).AsString(64).Nullable().
                    WithColumn((User x) => x.salt).AsString(5).Nullable().
                    WithColumn((User x) => x.name).AsString(200).NotNullable().
                    WithColumn((User x) => x.IdPhoto).AsInt32().Nullable().
                    WithColumn((User x) => x.Superuser).AsByte().NotNullable().WithDefaultValue(0).
                    WithColumn((User x) => x.State).AsInt16().NotNullable().WithDefaultValue(0).
                    WithColumn((User x) => x.StateConfirmation).AsString(100).Nullable().
                    WithColumn((User x) => x.AuthorizationAttempts).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((User x) => x.Block).AsInt16().NotNullable().WithDefaultValue(0).
                    WithColumn((User x) => x.BlockedUntil).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((User x) => x.BlockedReason).AsString(500).Nullable().
                    WithColumn((User x) => x.DateChange).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((User x) => x.IdUserChange).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((User x) => x.Comment).AsString(int.MaxValue).Nullable().
                    WithColumn((User x) => x.CommentAdmin).AsString(int.MaxValue).Nullable().
                    WithColumn((User x) => x.about).AsString(int.MaxValue).Nullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (User x) => x.IdUser, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (User x) => x.UniqueKey, x => x.AsString(200).Nullable());
                AddColumnIfNotExists(Schema, (User x) => x.email, x => x.AsString(128).Nullable());
                AddColumnIfNotExists(Schema, (User x) => x.phone, x => x.AsString(100).Nullable());
                AddColumnIfNotExists(Schema, (User x) => x.password, x => x.AsString(64).Nullable());
                AddColumnIfNotExists(Schema, (User x) => x.salt, x => x.AsString(5).Nullable());
                AddColumnIfNotExists(Schema, (User x) => x.name, x => x.AsString(200).NotNullable());
                AddColumnIfNotExists(Schema, (User x) => x.IdPhoto, x => x.AsInt32().Nullable());
                AddColumnIfNotExists(Schema, (User x) => x.Superuser, x => x.AsByte().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (User x) => x.State, x => x.AsInt16().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (User x) => x.StateConfirmation, x => x.AsString(100).Nullable());
                AddColumnIfNotExists(Schema, (User x) => x.AuthorizationAttempts, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (User x) => x.Block, x => x.AsInt16().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (User x) => x.BlockedUntil, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (User x) => x.BlockedReason, x => x.AsString(500).Nullable());
                AddColumnIfNotExists(Schema, (User x) => x.DateChange, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (User x) => x.IdUserChange, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (User x) => x.Comment, x => x.AsString(int.MaxValue).Nullable());
                AddColumnIfNotExists(Schema, (User x) => x.CommentAdmin, x => x.AsString(int.MaxValue).Nullable());
                AddColumnIfNotExists(Schema, (User x) => x.about, x => x.AsString(int.MaxValue).Nullable());
            }

            if (!Schema.Table<User>().Exists() || !Schema.Table<User>().Index($"t{GetTableName<User>()}_iUniqueKey").Exists())
                Create.Index($"t{GetTableName<User>()}_iUniqueKey").OnTable(GetTableName<User>()).
                    OnColumn(GetColumnName((User x) => x.UniqueKey)).Ascending().
                    WithOptions().UniqueNullsNotDistinct();

            if (!Schema.Table<User>().Exists() || !Schema.Table<User>().Index($"t{GetTableName<User>()}_iUniqueEmail").Exists())
                Create.Index($"t{GetTableName<User>()}_iUniqueEmail").OnTable(GetTableName<User>()).
                    OnColumn(GetColumnName((User x) => x.email)).Ascending().
                    WithOptions().UniqueNullsNotDistinct();

            Trigger_UsersUpsertHistory();
        }

        private void Trigger_UsersUpsertHistory()
        {
            var fieldsListUserHistory = new string[] {
                    GetColumnName((UserHistory x) => x.IdUser),
                    GetColumnName((UserHistory x) => x.UniqueKey),
                    GetColumnName((UserHistory x) => x.email),
                    GetColumnName((UserHistory x) => x.phone),
                    GetColumnName((UserHistory x) => x.password),
                    GetColumnName((UserHistory x) => x.salt),
                    GetColumnName((UserHistory x) => x.name),
                    GetColumnName((UserHistory x) => x.IdPhoto),
                    GetColumnName((UserHistory x) => x.Superuser),
                    GetColumnName((UserHistory x) => x.State),
                    GetColumnName((UserHistory x) => x.StateConfirmation),
                    GetColumnName((UserHistory x) => x.AuthorizationAttempts),
                    GetColumnName((UserHistory x) => x.Block),
                    GetColumnName((UserHistory x) => x.BlockedUntil),
                    GetColumnName((UserHistory x) => x.BlockedReason),
                    GetColumnName((UserHistory x) => x.DateChange),
                    GetColumnName((UserHistory x) => x.IdUserChange),
                    GetColumnName((UserHistory x) => x.Comment),
                    GetColumnName((UserHistory x) => x.CommentAdmin),
                    GetColumnName((UserHistory x) => x.about),
                    GetColumnName((UserHistory x) => x.DateChangeHistory),
                }.Select(x => $"[{x}]").ToList();

            var fieldsListUser = new string[] {
                    GetColumnName((User x) => x.IdUser),
                    GetColumnName((User x) => x.UniqueKey),
                    GetColumnName((User x) => x.email),
                    GetColumnName((User x) => x.phone),
                    GetColumnName((User x) => x.password),
                    GetColumnName((User x) => x.salt),
                    GetColumnName((User x) => x.name),
                    GetColumnName((User x) => x.IdPhoto),
                    GetColumnName((User x) => x.Superuser),
                    GetColumnName((User x) => x.State),
                    GetColumnName((User x) => x.StateConfirmation),
                    GetColumnName((User x) => x.AuthorizationAttempts),
                    GetColumnName((User x) => x.Block),
                    GetColumnName((User x) => x.BlockedUntil),
                    GetColumnName((User x) => x.BlockedReason),
                    GetColumnName((User x) => x.DateChange),
                    GetColumnName((User x) => x.IdUserChange),
                    GetColumnName((User x) => x.Comment),
                    GetColumnName((User x) => x.CommentAdmin),
                    GetColumnName((User x) => x.about),
                }.Select(x => $"[{x}]").ToList();

            var triggerBody = $@"
CREATE TRIGGER [dbo].[UsersUpsertHistory] ON  [dbo].[{GetTableName<User>()}] AFTER INSERT, UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	INSERT INTO [dbo].[{GetTableName<UserHistory>()}] ({string.Join(", ", fieldsListUserHistory)})
    SELECT {string.Join(", ", fieldsListUser)}, GETDATE()
	FROM inserted
END;
            ";

            IfDatabase("sqlserver").Execute.Sql($@"IF NOT EXISTS (SELECT * FROM sys.objects WHERE [type] = 'TR' AND [name] = 'UsersUpsertHistory') EXEC (N'{triggerBody.Replace("'", "''")}')");
        }
    }
}
