using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;
using OnUtils.Architecture.AppCore;
using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS0618
namespace OnXap.Core.DbSchema
{
    class DbSchemaManager : CoreComponentBase, IComponentSingleton, ICritical, IFilteringMigrationSource
    {
        #region CoreComponentBase
        protected sealed override void OnStart()
        {
            this.RegisterJournal("Журнал обслуживания схемы базы данных.");

            try
            {
                var serviceProvider = new ServiceCollection().
                    AddFluentMigratorCore().
                    ConfigureRunner(rb => rb.
                        AddSqlServer().
                        WithGlobalConnectionString(AppCore.ConnectionStringFactory())).
                    Configure<RunnerOptions>(cfg => cfg.Profile = DbSchemaDefaultProfile.ProfileName).
                    AddSingleton<IMigrationSource>(sp => this).
                    AddSingleton<IProfileSource, DbSchemaProfileSource>().
                    BuildServiceProvider(false);

                using (var scope = serviceProvider.CreateScope())
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
                    runner.MigrateUp();
                }
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.CriticalError, "Ошибка запуска миграций", null, ex);
            }
        }

        protected override void OnStop()
        {
        }
        #endregion

        #region IFilteringMigrationSource
        IEnumerable<IMigration> IFilteringMigrationSource.GetMigrations(Func<Type, bool> predicate)
        {
            var dbSchemaItemTypes = AppCore.GetQueryTypes().Where(x => typeof(DbSchemaItem).IsAssignableFrom(x));
            dbSchemaItemTypes = dbSchemaItemTypes.Where(predicate);
            var dbSchemaItemList = dbSchemaItemTypes.Select(x => AppCore.Create<DbSchemaItem>(x)).ToList();
            return dbSchemaItemList;
        }

        IEnumerable<IMigration> IMigrationSource.GetMigrations()
        {
            return Enumerable.Empty<IMigration>();
        }
        #endregion
    }
}
