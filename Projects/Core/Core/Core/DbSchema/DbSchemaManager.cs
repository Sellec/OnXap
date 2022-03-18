using FluentMigrator;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;
using OnUtils.Architecture.AppCore;
using System;
using FluentMigrator.Runner.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#pragma warning disable CS0618
namespace OnXap.Core.DbSchema
{
    class d : FluentMigrator.Runner.IMigrationRunnerConventions
    {
        Func<Type, bool> IMigrationRunnerConventions.TypeIsMigration => TypeIsMigrationImpl;

        Func<Type, bool> IMigrationRunnerConventions.TypeIsProfile => DefaultMigrationRunnerConventions.Instance.TypeIsProfile;

        Func<Type, MigrationStage?> IMigrationRunnerConventions.GetMaintenanceStage => DefaultMigrationRunnerConventions.Instance.GetMaintenanceStage;

        Func<Type, bool> IMigrationRunnerConventions.TypeIsVersionTableMetaData => DefaultMigrationRunnerConventions.Instance.TypeIsVersionTableMetaData;

        Func<Type, IMigrationInfo> IMigrationRunnerConventions.GetMigrationInfo => DefaultMigrationRunnerConventions.Instance.GetMigrationInfo;

        Func<IMigration, IMigrationInfo> IMigrationRunnerConventions.GetMigrationInfoForMigration => GetMigrationInfoForMigrationImpl;

        Func<Type, bool> IMigrationRunnerConventions.TypeHasTags => DefaultMigrationRunnerConventions.Instance.TypeHasTags;

        Func<Type, IEnumerable<string>, bool> IMigrationRunnerConventions.TypeHasMatchingTags => DefaultMigrationRunnerConventions.Instance.TypeHasMatchingTags;

        private static bool TypeIsMigrationImpl(Type type)
        {
            if (typeof(DbSchemaItem).IsAssignableFrom(type)) return true;
            return typeof(IMigration).IsAssignableFrom(type) && type.GetCustomAttributes<MigrationAttribute>(true).Any();
        }

        private static long number = long.MinValue;

        private static IMigrationInfo GetMigrationInfoForMigrationImpl(IMigration migration)
        {
            if (typeof(DbSchemaItem).IsAssignableFrom(migration.GetType()))
            {
                var migrationType = migration.GetType();
                var migrationAttribute = migrationType.GetCustomAttribute<MigrationAttribute>() ?? new MigrationAttribute(number++, "");
                var migrationInfo = new MigrationInfo(migrationAttribute.Version, migrationAttribute.Description, migrationAttribute.TransactionBehavior, migrationAttribute.BreakingChange, () => migration);

                foreach (var traitAttribute in migrationType.GetCustomAttributes<MigrationTraitAttribute>(true))
                    migrationInfo.AddTrait(traitAttribute.Name, traitAttribute.Value);

                return migrationInfo;
            }
            return DefaultMigrationRunnerConventions.Instance.GetMigrationInfoForMigration(migration);
        }
    }

    class DbSchemaManager : CoreComponentBase, IComponentSingleton, ICritical, IFilteringMigrationSource
    {
        private Exception[] _startErrors = null;

        #region CoreComponentBase
        protected sealed override void OnStarting()
        {
            if (!AppCore.Get<DbSchemaManagerConfigure>().IsSchemaControlEnabled) return;

            try
            {
                using (var ctx = new Db.CoreContextBase())
                {
                    var isNeedToRun = false;
                    var serviceProvider = new ServiceCollection().
                        AddFluentMigratorCore().
                        ConfigureRunner(rb =>
                        {
                            isNeedToRun = AppCore.DbConfigurationBuilder.OnConfigureFluentMigrator(rb);
                        }).
                        Configure<RunnerOptions>(cfg => cfg.Profile = DbSchemaDefaultProfile.ProfileName).
                        AddSingleton<IMigrationSource>(sp => this).
                        AddSingleton<IMigrationRunnerConventions>(sp => new d()).
                        AddSingleton<IProfileSource, DbSchemaProfileSource>().
                        BuildServiceProvider(false);

                    using (var scope = serviceProvider.CreateScope())
                    {
                        var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
                        if (isNeedToRun) runner.MigrateUp();
                    }
                }
            }
            catch (Exception ex)
            {
                _startErrors = new Exception[] { ex };
                Debug.WriteLine($"DbSchemaManager.OnStart: {ex}");
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

            var moved = new Dictionary<object, int>();
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
                    if (!moved.ContainsKey(item)) moved[item] = 0;
                    if (moved[item] > dbSchemaItemList.Count * dbSchemaItemList.Count) throw new InvalidProgramException($"Обнаружена циклическая зависимость типа '{item.GetType().FullName}' от некоторых других. Проверьте цепочку зависимости dependsOn.");
                    dbSchemaItemList.RemoveAt(i);
                    dbSchemaItemList.Insert(dependencyIndexMax, item);
                    moved[item]++;
                    i = -1;
                }
            }

            var dbSchemaItemListFiltered = dbSchemaItemList.Where(x => x.GetType() == typeof(DbSchemaDefaultMigration) ||
                                                                       x.GetType() == typeof(DbSchemaDefaultProfile) ||
                                                                       x.GetType() == typeof(Db.InsertOnDuplicateUpdateSchemaItem) ||
                                                                       dbSchemaManagerConfigure.FilterMigration(x)).ToList();

            var dupl = dbSchemaItemListFiltered.Where(x => x.GetType() == typeof(Db.InsertOnDuplicateUpdateSchemaItem)).ToList();
            dupl.ForEach(x => dbSchemaItemListFiltered.Remove(x));
            dupl.ForEach(x => dbSchemaItemListFiltered.Insert(0, x));

            return dbSchemaItemListFiltered;
        }

        IEnumerable<IMigration> IMigrationSource.GetMigrations()
        {
            return Enumerable.Empty<IMigration>();
        }
        #endregion

        public void WriteErrors()
        {
            try
            {
                this.RegisterJournal("Журнал обслуживания схемы базы данных.");
                _startErrors?.ForEach(ex => this.RegisterEvent(Journaling.EventType.CriticalError, "Ошибка запуска миграций", null, ex));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DbSchemaManager.WriteErrors: {ex}");
            }
        }
    }
}
