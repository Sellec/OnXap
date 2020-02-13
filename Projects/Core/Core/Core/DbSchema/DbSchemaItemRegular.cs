using FluentMigrator;
using System;

namespace OnXap.Core.DbSchema
{
    /// <summary>
    /// Общий класс для работы со схемой базы данных, экземпляры которого автоматически запускаются при каждом запуске ядра.
    /// </summary>
    [Profile(DbSchemaDefaultProfile.ProfileName)]
    public abstract class DbSchemaItemRegular : DbSchemaItem
    {
        /// <summary>
        /// См. <see cref="DbSchemaItem.DbSchemaItem(Type[])"/>.
        /// </summary>
        protected DbSchemaItemRegular(params Type[] schemaItemTypeDependsOn) : base(schemaItemTypeDependsOn)
        {
        }
    }

}
