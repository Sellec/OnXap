namespace OnXap.Core.DbSchema
{
    /// <summary>
    /// Используется для настройки менеджера контроля схемы базы данных.
    /// </summary>
    public class DbSchemaManagerConfigure : CoreComponentBase, IComponentSingleton
    {
        /// <summary>
        /// Может быть перегружен для переопределения настроек.
        /// </summary>
        protected override void OnStart()
        {
            IsSchemaControlEnabled = true;
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
        }

        /// <summary>
        /// Указывает, следует ли проводить контроль схемы базы данных.
        /// </summary>
        public bool IsSchemaControlEnabled { get; protected set; }

        /// <summary>
        /// Позволяет проводить дополнительную фильтрацию миграций.
        /// </summary>
        /// <remarks>Некоторые миграции требуют выполнения других миграций как зависимости. Контроль фильтрации таких миграций следует проводить вручную в данном методе.</remarks>
        public virtual bool FilterMigration(DbSchemaItem dbSchemaItem)
        {
            return true;
        }
    }
}
