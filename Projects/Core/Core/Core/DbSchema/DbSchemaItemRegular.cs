using FluentMigrator;

namespace OnXap.Core.DbSchema
{
    /// <summary>
    /// Общий класс для работы со схемой базы данных, экземпляры которого автоматически запускаются при запуске ядра.
    /// </summary>
    [Profile(DbSchemaDefaultProfile.ProfileName)]
    public abstract class DbSchemaItemRegular : DbSchemaItem
    {

    }
}
