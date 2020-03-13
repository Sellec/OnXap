using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;

namespace OnXap.Core.Data
{
    /// <summary>
    /// Позволяет сконфигурировать подключение к базе.
    /// </summary>
    public interface IDbConfigurationBuilder
    {
        /// <summary>
        /// Позволяет сконфигурировать подключение к базе в контекстах EntityFramework.
        /// </summary>
        void OnConfigureEntityFrameworkCore(DbContextOptionsBuilder optionsBuilder);

        /// <summary>
        /// Позволяет сконфигурировать подключение к базе для FluentMigrator. Если метод возвращает false, работа стандартного механизма миграций отключается.
        /// </summary>
        /// <seealso cref="DbSchema.DbSchemaManager"/>
        bool OnConfigureFluentMigrator(IMigrationRunnerBuilder runnerBuilder);
    }
}
