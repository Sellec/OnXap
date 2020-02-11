using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Builders.Schema.Column;
using FluentMigrator.Builders.Schema.Table;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace OnXap.Core.DbSchema
{
    /// <summary>
    /// Методы расширения для FluentMigrator.
    /// </summary>
    public static class FluentMigratorColumnExtensions
    {
        /// <summary>
        /// Возвращает имя столбца на основе свойства или поля, переданного в выражении <paramref name="columnAccessor"/>. 
        /// </summary>
        public static string GetColumnName<TTable, TProperty>(Expression<Func<TTable, TProperty>> columnAccessor)
        {
            if (columnAccessor.Body is MemberExpression memberExpression)
            {
                var columnName = memberExpression.Member.GetCustomAttribute<ColumnAttribute>(true)?.Name;
                if (!string.IsNullOrEmpty(columnName)) return columnName;

                if (memberExpression.Member is PropertyInfo propertyInfo) return propertyInfo.Name;
                if (memberExpression.Member is FieldInfo fieldInfo) return fieldInfo.Name;
            }

            throw new ArgumentException("Должно быть обращением к свойству или полю.");
        }

        /// <summary>
        /// Определяет новый столбец с именем на основе свойства или поля, переданного в выражении <paramref name="columnAccessor"/>. 
        /// </summary>
        /// <returns>См. <see cref="ICreateTableWithColumnSyntax.WithColumn(string)"/>.</returns>
        /// <remarks>В качестве имени столбцы используется значение атрибута <see cref="ColumnAttribute"/> или имя свойства/типа из выражения <paramref name="columnAccessor"/>.</remarks>
        public static ICreateTableColumnAsTypeSyntax WithColumn<TTable, TProperty>(this ICreateTableWithColumnSyntax table, Expression<Func<TTable, TProperty>> columnAccessor)
        {
            return table.WithColumn(GetColumnName(columnAccessor));
        }

        /// <summary>
        /// Определяет столбец для проверки на существование с именем на основе свойства или поля, переданного в выражении <paramref name="columnAccessor"/>.
        /// </summary>
        /// <returns>См. <see cref="ISchemaTableSyntax.Column(string)"/>.</returns>
        /// <remarks>В качестве имени столбцы используется значение атрибута <see cref="ColumnAttribute"/> или имя свойства/типа из выражения <paramref name="columnAccessor"/>.</remarks>
        public static ISchemaColumnSyntax Column<TTable, TProperty>(this ISchemaTableSyntax table, Expression<Func<TTable, TProperty>> columnAccessor)
        {
            return table.Column(GetColumnName(columnAccessor));
        }

        /// <summary>
        /// Добавляет столбец в талицу с именем на основе свойства или поля, переданного в выражении <paramref name="columnAccessor"/>.
        /// </summary>
        /// <returns>См. <see cref="IAlterTableAddColumnOrAlterColumnSyntax.AddColumn(string)"/>.</returns>
        /// <remarks>В качестве имени столбцы используется значение атрибута <see cref="ColumnAttribute"/> или имя свойства/типа из выражения <paramref name="columnAccessor"/>.</remarks>
        public static IAlterTableColumnAsTypeSyntax AddColumn<TTable, TProperty>(this IAlterTableAddColumnOrAlterColumnSyntax table, Expression<Func<TTable, TProperty>> columnAccessor)
        {
            return table.AddColumn(GetColumnName(columnAccessor));
        }

    }
}
