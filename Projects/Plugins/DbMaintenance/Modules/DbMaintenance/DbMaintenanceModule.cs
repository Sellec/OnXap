using OnUtils.Architecture.AppCore;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace OnXap.Modules.DbMaintenance
{
    using Core.Modules;
    using Db;
    using Journaling;
    using TaskSheduling;
    using Types;

    /// <summary>
    /// Менеджер для работы со словарными формами.
    /// </summary>
    [ModuleCore("Обслуживание базы данных")]
    public class DbMaintenanceModule : ModuleCore<DbMaintenanceModule>
    {
        private static DbMaintenanceModule _thisModule = null;
        private static ConcurrentFlagLocker<string> _servicesFlags = new ConcurrentFlagLocker<string>();

        /// <summary>
        /// </summary>
        protected override void OnModuleStarting()
        {
            _thisModule = this;

            var taskSchedulingManager = AppCore.Get<TaskSchedulingManager>();
            var task = taskSchedulingManager.RegisterTask(new TaskRequest()
            {
                Name = "Обслуживание индексов",
                Description = "",
                IsEnabled = true,
                TaskOptions = TaskOptions.AllowDisabling | TaskOptions.AllowManualSchedule | TaskOptions.PreventParallelExecution,
                UniqueKey = $"{typeof(DbMaintenanceModule).FullName}_{nameof(MaintenanceIndexes)}",
                ExecutionLambda = () => MaintenanceIndexesStatic()
            });
            if (task.ManualSchedules.Count == 0) taskSchedulingManager.SetTaskManualScheduleList(task, new List<TaskSchedule>() { new TaskCronSchedule(Cron.Daily()) { IsEnabled = true } });
        }

        #region Maintenance indexes tasks
        [ApiIrreversible]
        public static void MaintenanceIndexesStatic()
        {
            var module = _thisModule;
            if (module == null) throw new Exception("Модуль не найден.");

            module.MaintenanceIndexes();
        }

        private void MaintenanceIndexes()
        {
            if (AppCore.GetState() != CoreComponentState.Started) return;

            try
            {
                using (var db = new DataContext())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress))
                {
                    db.QueryTimeout = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;
                    var result = db.StoredProcedure<object>("Maintenance_RebuildIndexes", new { MinimumIndexFragmentstionToSearch = 5 });
                }
            }
            catch (Exception ex)
            {
                this.RegisterEvent(EventType.CriticalError, $"Ошибка обслуживания индексов", null, ex);
                Debug.WriteLine("FileManager.Module.MaintenanceIndexes: {0}", ex.Message);
            }
        }
        #endregion

    }
}


