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
                    WithColumn((User x) => x.name).AsString(200).Nullable().
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

                IfDatabase("sqlserver").Execute.Sql($"CREATE UNIQUE NONCLUSTERED INDEX [UniqueKey] ON [{FluentMigratorTableExtensions.GetTableName<User>()}] ([{FluentMigratorColumnExtensions.GetColumnName((User x) => x.UniqueKey)}] ASC) WHERE ([{FluentMigratorColumnExtensions.GetColumnName((User x) => x.UniqueKey)}] IS NOT NULL);");
                IfDatabase("sqlserver").Execute.Sql($"CREATE UNIQUE NONCLUSTERED INDEX [UniqueEmail] ON [{FluentMigratorTableExtensions.GetTableName<User>()}] ([{FluentMigratorColumnExtensions.GetColumnName((User x) => x.email)}] ASC) WHERE ([{FluentMigratorColumnExtensions.GetColumnName((User x) => x.email)}] IS NOT NULL);");

            }
            else
            {
                AddColumnIfNotExists(Schema, (User x) => x.IdUser, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (User x) => x.UniqueKey, x => x.AsString(200).Nullable());
                AddColumnIfNotExists(Schema, (User x) => x.email, x => x.AsString(128).Nullable());
                AddColumnIfNotExists(Schema, (User x) => x.phone, x => x.AsString(100).Nullable());
                AddColumnIfNotExists(Schema, (User x) => x.password, x => x.AsString(64).Nullable());
                AddColumnIfNotExists(Schema, (User x) => x.salt, x => x.AsString(5).Nullable());
                AddColumnIfNotExists(Schema, (User x) => x.name, x => x.AsString(200).Nullable());
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

                if (!Schema.Table<User>().Index("UniqueKey").Exists()) IfDatabase("sqlserver").Execute.Sql($"CREATE UNIQUE NONCLUSTERED INDEX [UniqueKey] ON [{FluentMigratorTableExtensions.GetTableName<User>()}] ([{FluentMigratorColumnExtensions.GetColumnName((User x) => x.UniqueKey)}] ASC) WHERE ([{FluentMigratorColumnExtensions.GetColumnName((User x) => x.UniqueKey)}] IS NOT NULL);");
                if (!Schema.Table<User>().Index("UniqueEmail").Exists()) IfDatabase("sqlserver").Execute.Sql($"CREATE UNIQUE NONCLUSTERED INDEX [UniqueEmail] ON [{FluentMigratorTableExtensions.GetTableName<User>()}] ([{FluentMigratorColumnExtensions.GetColumnName((User x) => x.email)}] ASC) WHERE ([{FluentMigratorColumnExtensions.GetColumnName((User x) => x.email)}] IS NOT NULL);");
            }

            Trigger_UsersUpsertHistory();
        }

        private void Trigger_UsersUpsertHistory()
        {
            var fieldsListUserHistory = new string[] {
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.IdUser),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.UniqueKey),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.email),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.phone),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.password),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.salt),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.name),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.IdPhoto),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.Superuser),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.State),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.StateConfirmation),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.AuthorizationAttempts),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.Block),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.BlockedUntil),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.BlockedReason),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.DateChange),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.IdUserChange),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.Comment),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.CommentAdmin),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.about),
                    FluentMigratorColumnExtensions.GetColumnName((UserHistory x) => x.DateChangeHistory),
                }.Select(x => $"[{x}]").ToList();

            var fieldsListUser = new string[] {
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.IdUser),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.UniqueKey),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.email),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.phone),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.password),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.salt),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.name),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.IdPhoto),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.Superuser),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.State),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.StateConfirmation),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.AuthorizationAttempts),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.Block),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.BlockedUntil),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.BlockedReason),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.DateChange),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.IdUserChange),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.Comment),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.CommentAdmin),
                    FluentMigratorColumnExtensions.GetColumnName((User x) => x.about),
                }.Select(x => $"[{x}]").ToList();

            var triggerBody = $@"
CREATE TRIGGER [dbo].[UsersUpsertHistory] ON  [dbo].[{FluentMigratorTableExtensions.GetTableName<User>()}] AFTER INSERT, UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	INSERT INTO [dbo].[{FluentMigratorTableExtensions.GetTableName<UserHistory>()}] ({string.Join(", ", fieldsListUserHistory)})
    SELECT {string.Join(", ", fieldsListUser)}, GETDATE()
	FROM inserted
END;
            ";

            IfDatabase("sqlserver").Execute.Sql($@"IF NOT EXISTS (SELECT * FROM sys.objects WHERE [type] = 'TR' AND [name] = 'UsersUpsertHistory') EXEC (N'{triggerBody.Replace("'", "''")}')");
        }
    }
}
