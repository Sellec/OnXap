using OnUtils.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OnXap.TaskSheduling
{
    /// <summary>
    /// Позволяет управлять задачами.
    /// </summary>
    public class TaskSchedulingManager : Core.CoreComponentBase, Core.IComponentSingleton
    {
        private static TaskSchedulingManager _this;
        private ConcurrentDictionary<string, TaskDescription> _taskList = new ConcurrentDictionary<string, TaskDescription>();

        #region Управление задачами.
        /// <summary>
        /// Позволяет зарегистрировать задачу. Если задача ранее была зарегистрирована, то обновляет параметры ранее зарегистрированной задачи.
        /// </summary>
        /// <returns>Возвращает зарегистрированную задачу.</returns>
        public TaskDescription RegisterTask(TaskRequest taskRequest)
        {
            if (taskRequest == null) throw new ArgumentNullException(nameof(taskRequest));
            if (string.IsNullOrEmpty(taskRequest.Name)) throw new ArgumentException("Имя задачи не может быть пустым.", nameof(taskRequest.Name));
            if (string.IsNullOrEmpty(taskRequest.UniqueKey)) throw new ArgumentException("Уникальный ключ задачи не может быть пустым.", nameof(taskRequest.UniqueKey));

            var taskDescription = _taskList.AddOrUpdate(taskRequest.UniqueKey,
                key => {
                    return new TaskDescription()
                    {
                        Id = -1,
                        Name = taskRequest.Name,
                        Description = taskRequest.Description,
                        ExecutionLambda = taskRequest.ExecutionLambda,
                        IsConfirmed = true,
                        UniqueKey = taskRequest.UniqueKey,
                        AllowManualShedule = taskRequest.AllowManualShedule,
                        Schedules = new List<TaskSchedule>(taskRequest.Schedules)
                    };
                },
                (key, old) =>
                {
                    old.Id = -1;
                    old.Name = taskRequest.Name;
                    old.Description = taskRequest.Description;
                    old.ExecutionLambda = taskRequest.ExecutionLambda;
                    old.IsConfirmed = true;
                    old.UniqueKey = taskRequest.UniqueKey;
                    old.AllowManualShedule = taskRequest.AllowManualShedule;
                    old.Schedules = new List<TaskSchedule>(taskRequest.Schedules);
                    return old;
                });
            PrepareTaskSchedules(taskDescription);
            return taskDescription;
        }

        private void PrepareTaskSchedules(TaskDescription taskDescription)
        {
            int i = 0;
            foreach(var schedule in taskDescription.Schedules)
            {
                if (schedule is TaskCronSchedule taskCronSchedule)
                {
                    var scheduleUniqueKey = $"task_{taskDescription.UniqueKey}_sc_{i}";
                    TasksManager.SetTask(scheduleUniqueKey, taskCronSchedule.CronExpression, () => ExecuteTaskStatic(taskDescription.UniqueKey));
                }
                else if (schedule is TaskFixedTimeSchedule taskFixedTimeSchedule)
                {
                    var scheduleUniqueKey = $"task_{taskDescription.UniqueKey}_sc_{i}";
                    TasksManager.SetTask(scheduleUniqueKey, taskFixedTimeSchedule.DateTime, () => ExecuteTaskStatic(taskDescription.UniqueKey));
                }
            }
        }
        #endregion

        #region Запуск задач
        /// <summary>
        /// </summary>
        protected sealed override void OnStarting()
        {
            this.RegisterJournal("Журнал планировщика задач");
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStarted()
        {
            _this = this;
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
            if (_this == this) _this = null;
        }

        private void ExecuteTask(string uniqueKey)
        {
            if (!_taskList.TryGetValue(uniqueKey, out var taskDescription))
            {
                this.RegisterEvent(Journaling.EventType.Error, "Ошибка запуска задачи - задача не найдена", $"Ключ задачи - '{uniqueKey}'.");
                return;
            }

            try
            {
                this.RegisterEvent(Journaling.EventType.Info, "Запуск задачи", $"Запуск задачи '{taskDescription.Name}' (№{taskDescription.Id} / '{taskDescription.UniqueKey}').");
                var timeStart = DateTime.Now;

                taskDescription.ExecutionLambda.Compile().Invoke();

                this.RegisterEvent(Journaling.EventType.Info, "Завершение задачи", $"Задача '{taskDescription.Name}' (№{taskDescription.Id} / '{taskDescription.UniqueKey}') выполнена за {Math.Round((DateTime.Now - timeStart).TotalSeconds, 3)} сек.");
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.Info, "Ошибка выполнения задачи", $"Неожиданная ошибка выполнения задачи '{taskDescription.Name}' (№{taskDescription.Id} / '{taskDescription.UniqueKey}').", ex);
            }
        }

        private static void ExecuteTaskStatic(string uniqueKey)
        {
            _this?.ExecuteTask(uniqueKey);
        }
        #endregion
    }
}
