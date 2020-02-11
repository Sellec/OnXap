using FluentMigrator.Builders.Alter;
using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Builders.Schema;
using FluentMigrator.Builders.Schema.Table;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace OnXap.Core.DbSchema
{
    /// <summary>
    /// Методы расширения для FluentMigrator.
    /// </summary>
    public static class FluentMigratorTableExtensions
    {
        private static ConcurrentDictionary<Type, string> _tableCache = new ConcurrentDictionary<Type, string>();

        /// <summary>
        /// Возвращает имя таблицы на основе типа <typeparamref name="TTable"/>. 
        /// </summary>
        /// <typeparam name="TTable"></typeparam>
        public static string GetTableName<TTable>()
        {
            return _tableCache.GetOrAdd(typeof(TTable), type => type.GetCustomAttribute<TableAttribute>(true)?.Name ?? type.Name);
        }

        /// <summary>
        /// Создает таблицу с именем на основе типа <typeparamref name="TTable"/>. 
        /// </summary>
        /// <returns>См. <see cref="ICreateExpressionRoot.Table(string)"/>.</returns>
        /// <remarks>В качестве имени таблицы используется значение атрибута <see cref="TableAttribute"/> или имя типа в единственном числе.</remarks>
        public static ICreateTableWithColumnOrSchemaOrDescriptionSyntax Table<TTable>(this ICreateExpressionRoot create)
        {
            return create.Table(GetTableName<TTable>());
        }

        /// <summary>
        /// Определяет таблицу с именем на основе типа <typeparamref name="TTable"/> как базу для определения существования объектов базы данных.
        /// </summary>
        /// <returns>См. <see cref="ISchemaExpressionRoot.Table(string)"/>.</returns>
        /// <remarks>В качестве имени таблицы используется значение атрибута <see cref="TableAttribute"/> или имя типа в единственном числе.</remarks>
        public static ISchemaTableSyntax Table<TTable>(this ISchemaExpressionRoot schema)
        {
            return schema.Table(GetTableName<TTable>());
        }

        /// <summary>
        /// Выбирает таблицу с именем на основе типа <typeparamref name="TTable"/> или её столбцы.
        /// </summary>
        /// <returns>См. <see cref="IAlterExpressionRoot.Table(string)"/>.</returns>
        /// <remarks>В качестве имени таблицы используется значение атрибута <see cref="TableAttribute"/> или имя типа в единственном числе.</remarks>
        public static IAlterTableAddColumnOrAlterColumnOrSchemaOrDescriptionSyntax Table<TTable>(this IAlterExpressionRoot alter)
        {
            return alter.Table(GetTableName<TTable>());
        }

    }
}
