using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentMigrator;

namespace OnXap.Modules.FileManager.DbSchema
{
    using Core.DbSchema;

    [Migration(20200306190500, "Исправление длины поля File.TypeConcrete.")]
    public class File20200306 : DbSchemaItem
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            Alter.
                Column(GetColumnName((Db.File x) => x.TypeConcrete)).
                OnTable(GetTableName<Db.File>()).
                AsString(100).Nullable();
        }
    }
}
