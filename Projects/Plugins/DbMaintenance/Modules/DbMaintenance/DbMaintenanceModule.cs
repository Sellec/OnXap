using OnUtils.Architecture.AppCore;
using OnUtils.Tasks;
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

            /*
             * Обслуживание индексов запускаем один раз при старте и раз в несколько часов
             * */
            AppCore.Get<TaskSchedulingManager>().RegisterTask(new TaskRequest()
            {
                Name = "Обслуживание индексов",
                Description = "",
                AllowManualShedule = false,
                UniqueKey = $"{typeof(DbMaintenanceModule).FullName}_{nameof(MaintenanceIndexes)}",
                ExecutionLambda = () => MaintenanceIndexesStatic(),
                Schedules = new List<TaskSchedule>() { new TaskCronSchedule(Cron.HourInterval(4)) }
            });
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


