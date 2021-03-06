﻿using FluentMigrator.SqlServer;

namespace OnXap.Core.Db
{
    using DbSchema;

    class RoleSchemaItem : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            if (!Schema.Table<Role>().Exists())
            {
                Create.Table<Role>().
                    WithColumn((Role x) => x.IdRole).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((Role x) => x.NameRole).AsString(200).NotNullable().WithDefaultValue("").
                    WithColumn((Role x) => x.IsHidden).AsBoolean().NotNullable().WithDefaultValue(false).
                    WithColumn((Role x) => x.IdUserCreate).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((Role x) => x.DateCreate).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((Role x) => x.IdUserChange).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((Role x) => x.DateChange).AsInt32().NotNullable().WithDefaultValue(0).
                    WithColumn((Role x) => x.UniqueKey).AsString(100).Nullable();
            }
            else
            {
                AddColumnIfNotExists(Schema, (Role x) => x.IdRole, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (Role x) => x.NameRole, x => x.AsString(200).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (Role x) => x.IsHidden, x => x.AsBoolean().NotNullable().WithDefaultValue(false));
                AddColumnIfNotExists(Schema, (Role x) => x.IdUserCreate, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (Role x) => x.DateCreate, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (Role x) => x.IdUserChange, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (Role x) => x.DateChange, x => x.AsInt32().NotNullable().WithDefaultValue(0));
                AddColumnIfNotExists(Schema, (Role x) => x.UniqueKey, x => x.AsString(100).Nullable());
            }

            if (!Schema.Table<Role>().Exists() || !Schema.Table<Role>().Index($"t{GetTableName<Role>()}_iUniqueKey").Exists())
                Create.Index($"t{GetTableName<Role>()}_iUniqueKey").OnTable(GetTableName<Role>()).
                    OnColumn(GetColumnName((Role x) => x.UniqueKey)).Ascending().
                    WithOptions().UniqueNullsNotDistinct();
        }
    }
}
