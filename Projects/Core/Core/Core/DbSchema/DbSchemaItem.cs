using FluentMigrator;
using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Builders.Schema;
using OnUtils.Architecture.AppCore;
using System;
using System.Linq.Expressions;

namespace OnXap.Core.DbSchema
{
    /// <summary>
    /// Общий класс для работы со схемой базы данных.
    /// </summary>
    /// <seealso cref="Migration"/>
    /// <seealso cref="MigrationAttribute"/>
    /// <seealso cref="ProfileAttribute"/>
    public abstract class DbSchemaItem : Migration, IComponentTransient
    {
        class ComponentTransientImpl : CoreComponentBase, IComponentTransient
        {
            private readonly DbSchemaItem _instance;

            public ComponentTransientImpl(DbSchemaItem instance)
            {
                _instance = instance;
            }

            protected override void OnStarting()
            {
            }

            protected override void OnStop()
            {
            }
        }

        private readonly ComponentTransientImpl _componentImpl;

        /// <summary>
        /// Создает новый экземпляр объекта.
        /// </summary>
        /// <param name="schemaItemTypeDependsOn">Список типов миграций, от которых зависит текущий тип. Обработчик миграций выстроит правильный порядок выполнения миграций, исходя из зависимостей.</param>
        /// <exception cref="ArgumentException">Возникает, если тип <paramref name="schemaItemTypeDependsOn"/> не наследуется от <see cref="Migration"/>.</exception>
        protected DbSchemaItem(params Type[] schemaItemTypeDependsOn)
        {
            foreach (var type in schemaItemTypeDependsOn)
            {
                if (!typeof(Migration).IsAssignableFrom(type)) throw new ArgumentException(nameof(schemaItemTypeDependsOn));
            }

            _componentImpl = new ComponentTransientImpl(this);
            SchemaItemTypeDependsOn = schemaItemTypeDependsOn;
        }

        #region IComponentTransient
        /// <summary>
        /// </summary>
        public void Stop()
        {
            ((IComponentTransient)_componentImpl).Stop();
        }

        /// <summary>
        /// </summary>
        public CoreComponentState GetState()
        {
            return _componentImpl.GetState();
        }

        /// <summary>
        /// </summary>
        public OnXApplication GetAppCore()
        {
            return _componentImpl.GetAppCore();
        }

        /// <summary>
        /// </summary>
        public void Start(OnXApplication appCore)
        {
            ((IComponentTransient)_componentImpl).Start(appCore);
        }
        #endregion

        #region Методы для работы с FluentMigrator
        /// <summary>
        /// Добавляет новый столбец с именем на основе свойства или поля, переданного в выражении <paramref name="columnAccessor"/>, в таблицу с именем на основе типа <typeparamref name="TTable"/> в схеме <paramref name="schema"/>, если такой столбец не существует.
        /// При добавлении столбца вызывается лямбда-метод <paramref name="columnFluentCallback"/> для определения дополнительных признаков столбца.
        /// </summary>
        protected void AddColumnIfNotExists<TTable, TProperty>(ISchemaExpressionRoot schema, Expression<Func<TTable, TProperty>> columnAccessor, Action<IAlterTableColumnAsTypeSyntax> columnFluentCallback)
        {
            var prop = typeof(MigrationBase).GetProperty("Context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var context = prop.GetValue(this) as FluentMigrator.Infrastructure.IMigrationContext;
            if (context.QuerySchema.DatabaseType.ToLower() == "sqlite") return;
            if (!Schema.Table<TTable>().Column(columnAccessor).Exists()) columnFluentCallback(Alter.Table<TTable>().AddColumn(columnAccessor));
        }

        /// <summary>
        /// Возвращает имя столбца на основе свойства или поля, переданного в выражении <paramref name="columnAccessor"/>. 
        /// </summary>
        /// <seealso cref="FluentMigratorColumnExtensions.GetColumnName{TTable, TProperty}(Expression{Func{TTable, TProperty}})"/>
        protected string GetColumnName<TTable, TProperty>(Expression<Func<TTable, TProperty>> columnAccessor)
        {
            return FluentMigratorColumnExtensions.GetColumnName(columnAccessor);
        }

        /// <summary>
        /// Возвращает имя таблицы на основе типа <typeparamref name="TTable"/>. 
        /// </summary>
        /// <seealso cref="FluentMigratorTableExtensions.GetTableName{TTable}"/>
        protected string GetTableName<TTable>()
        {
            return FluentMigratorTableExtensions.GetTableName<TTable>();
        }
        #endregion

        /// <summary>
        /// Список типов миграций, от которых зависит текущая миграция.
        /// </summary>
        public Type[] SchemaItemTypeDependsOn { get; }
    }
}
