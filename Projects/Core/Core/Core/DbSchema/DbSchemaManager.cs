using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;
using OnUtils.Architecture.AppCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnXap.Core.DbSchema
{
    class DbSchemaManager : CoreComponentBase, IComponentSingleton, ICritical, IFilteringMigrationSource
    {
        #region CoreComponentBase
        protected sealed override void OnStart()
        {
            if (!AppCore.Get<DbSchemaManagerConfigure>().IsSchemaControlEnabled) return;

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
                try
                {
                    this.RegisterJournal("Журнал обслуживания схемы базы данных.");
                    this.RegisterEvent(Journaling.EventType.CriticalError, "Ошибка запуска миграций", null, ex);
                }
                catch { }
            }
        }

        protected override void OnStop()
        {
        }
        #endregion

        #region IFilteringMigrationSource
        IEnumerable<IMigration> IFilteringMigrationSource.GetMigrations(Func<Type, bool> predicate)
        {
            var dbSchemaManagerConfigure = AppCore.Get<DbSchemaManagerConfigure>();
            var dbSchemaItemTypes = AppCore.GetQueryTypes().Where(x => typeof(DbSchemaItem).IsAssignableFrom(x));
            dbSchemaItemTypes = dbSchemaItemTypes.Where(predicate);
            var dbSchemaItemList = dbSchemaItemTypes.Select(x => AppCore.Create<DbSchemaItem>(x)).OrderBy(x => x is DbSchemaItem).ToList();

            var moved = new List<object>();
            for (int i = 0; i < dbSchemaItemList.Count; i++)
            {
                var item = dbSchemaItemList[i];
                if (!(item is DbSchemaItem schemaItem)) continue;
                var dependsOn = schemaItem.SchemaItemTypeDependsOn;
                if (dependsOn.IsNullOrEmpty()) continue;

                int dependencyIndexMax = -1;
                foreach (var dependencyType in dependsOn)
                {
                    var isFound = false;
                    for (int j = 0; j < dbSchemaItemList.Count; j++)
                    {
                        var itemDependency = dbSchemaItemList[j];
                        if (itemDependency.GetType() == dependencyType)
                        {
                            isFound = true;
                            dependencyIndexMax = Math.Max(j, dependencyIndexMax);
                            break;
                        }
                    }
                    if (!isFound) throw new InvalidProgramException($"Не найдена зависимость '{dependencyType.FullName}' для миграции '{schemaItem.GetType().FullName}'.");
                }
                if (dependencyIndexMax > i)
                {
                    if (moved.Contains(item)) throw new InvalidProgramException($"Обнаружена циклическая зависимость типа '{item.GetType().FullName}' от некоторых других. Проверьте цепочку зависимости dependsOn.");
                    dbSchemaItemList.RemoveAt(i);
                    dbSchemaItemList.Insert(dependencyIndexMax, item);
                    moved.Add(item);
                    i = -1;
                }
            }

            var dbSchemaItemListFiltered = dbSchemaItemList.Where(x => x.GetType() == typeof(DbSchemaDefaultMigration) ||
                                                                       x.GetType() == typeof(DbSchemaDefaultProfile) ||
                                                                       dbSchemaManagerConfigure.FilterMigration(x)).ToList();
            return dbSchemaItemListFiltered;
        }

        IEnumerable<IMigration> IMigrationSource.GetMigrations()
        {
            return Enumerable.Empty<IMigration>();
        }
        #endregion
    }
}
