using Microsoft.EntityFrameworkCore;
using NCrontab;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace OnXap.TaskSheduling
{
    /// <summary>
    /// Позволяет управлять задачами.
    /// </summary>
    public class TaskSchedulingManager : Core.CoreComponentBase, Core.IComponentSingleton
    {
        private static TaskSchedulingManager _this;
        private ConcurrentDictionary<string, TaskDescription> _taskList = new ConcurrentDictionary<string, TaskDescription>();
        private Types.ConcurrentFlagLocker<string> _executeFlags = new Types.ConcurrentFlagLocker<string>();

        private Timer _jobsTimer = null;
        private object _jobsSyncRoot = new object();
        private List<JobInternal> _jobsList = new List<JobInternal>();
        private Guid _unique = Guid.NewGuid();
        private List<Action> _delayedTaskRegistration = new List<Action>();

        #region Управление задачами.
        /// <summary>
        /// Позволяет зарегистрировать задачу. Если задача ранее была зарегистрирована, то обновляет параметры ранее зарегистрированной задачи.
        /// </summary>
        /// <returns>Возвращает зарегистрированную задачу.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="taskRequest"/> равен null.</exception>
        /// <exception cref="ArgumentException">Возникает, если не указано имя задачи.</exception>
        /// <exception cref="ArgumentException">Возникает, если не указан уникальный ключ задачи.</exception>
        public TaskDescription RegisterTask(TaskRequest taskRequest)
        {
            if (taskRequest == null) throw new ArgumentNullException(nameof(taskRequest));
            if (string.IsNullOrEmpty(taskRequest.Name)) throw new ArgumentException("Имя задачи не может быть пустым.", nameof(taskRequest.Name));
            if (string.IsNullOrEmpty(taskRequest.UniqueKey)) throw new ArgumentException("Уникальный ключ задачи не может быть пустым.", nameof(taskRequest.UniqueKey));
            if (taskRequest.Schedules?.GroupBy(x=>x.GetUniqueKey()).Any(x=>x.Count() > 1) ?? false) throw new ArgumentException("В задаче есть повторяющиеся правила запуска.", nameof(taskRequest.Schedules));

            try
            {
                var taskDescription = _taskList.AddOrUpdate(taskRequest.UniqueKey,
                    key => UpdateTask(new TaskDescription(), taskRequest),
                    (key, old) => UpdateTask(old, taskRequest)
                );
                PrepareTaskSchedules(taskDescription);
                return taskDescription;
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.CriticalError, "Ошибка во время регистрации задачи", null, ex);
                throw new Exception("Неожиданная ошибка во время регистрации задачи.");
            }
        }

        private TaskDescription UpdateTask(TaskDescription taskDescription, TaskRequest taskRequest)
        {
            using (var db = new Db.DataContext())
            {
                var taskDb = db.Task.Where(x => x.UniqueKey == taskRequest.UniqueKey).Include(x => x.TaskSchedules).FirstOrDefault();
                if (taskDb == null)
                {
                    taskDb = new Db.Task()
                    {
                        Name = taskRequest.Name,
                        Description = taskRequest.Description,
                        IsEnabled = null,
                        TaskSchedules = new List<Db.TaskSchedule>(),
                        UniqueKey = taskRequest.UniqueKey
                    };
                    db.Task.Add(taskDb);
                    db.SaveChanges();
                }

                taskDb.Name = taskRequest.Name;
                taskDb.Description = taskRequest.Description;

                if (!taskRequest.TaskOptions.HasFlag(TaskOptions.AllowDisabling))
                {
                    taskDb.IsEnabled = null;
                }

                if (!taskRequest.TaskOptions.HasFlag(TaskOptions.AllowManualSchedule) && taskDb.TaskSchedules.Count > 0)
                {
                    db.TaskSchedule.RemoveRange(taskDb.TaskSchedules);
                    taskDb.TaskSchedules.Clear();
                }
                db.SaveChanges();

                var schedules = new List<TaskSchedule>();
                foreach (var scheduleDb in taskDb.TaskSchedules)
                {
                    TaskSchedule taskSchedule = null;
                    if (scheduleDb.DateTimeFixed.HasValue)
                    {
                        taskSchedule = new TaskFixedTimeSchedule(new DateTimeOffset(scheduleDb.DateTimeFixed.Value.Ticks, TimeSpan.Zero));
                    }
                    else if (!string.IsNullOrEmpty(scheduleDb.Cron))
                    {
                        taskSchedule = new TaskCronSchedule(scheduleDb.Cron);
                    }

                    if (taskSchedule == null)
                    {
                        continue;
                    }
                    taskSchedule.IsEnabled = scheduleDb.IsEnabled;
                    schedules.Add(taskSchedule);
                }

                taskDescription.Id = taskDb.Id;
                taskDescription.Name = taskRequest.Name;
                taskDescription.Description = taskRequest.Description;
                taskDescription.ExecutionLambda = taskRequest.ExecutionLambda;
                taskDescription.IsConfirmed = true;
                taskDescription.UniqueKey = taskRequest.UniqueKey;
                taskDescription.IsEnabled = taskDb.IsEnabled ?? taskRequest.IsEnabled;
                taskDescription.TaskOptions = taskRequest.TaskOptions;
                taskDescription.Schedules = new ReadOnlyCollection<TaskSchedule>(taskRequest.Schedules ?? new List<TaskSchedule>());
                taskDescription.ManualSchedules = new ReadOnlyCollection<TaskSchedule>(schedules.GroupBy(x => x.GetUniqueKey()).Select(x=>x.First()).ToList());
            }
            return taskDescription;
        }

        /// <summary>
        /// Возвращает список задач.
        /// </summary>
        /// <param name="onlyConfirmed">Если равено true, то возвращает только подтвержденные задачи (см. <see cref="TaskDescription.IsConfirmed"/>.</param>
        public List<TaskDescription> GetTaskList(bool onlyConfirmed)
        {
            try
            {
                var list = _taskList.Values.ToDictionary(x => x.Id, x => x);
                if (!onlyConfirmed)
                {
                    using (var db = new Db.DataContext())
                    {
                        var query = db.Task.Include(x => x.TaskSchedules);
                        var taskList = query.ToList();
                        foreach (var taskDb in taskList)
                        {
                            if (list.ContainsKey(taskDb.Id)) continue;

                            var schedules = new List<TaskSchedule>();
                            foreach (var scheduleDb in taskDb.TaskSchedules)
                            {
                                TaskSchedule taskSchedule = null;
                                if (scheduleDb.DateTimeFixed.HasValue)
                                {
                                    taskSchedule = new TaskFixedTimeSchedule(new DateTimeOffset(scheduleDb.DateTimeFixed.Value.Ticks, TimeSpan.Zero));
                                }
                                else if (!string.IsNullOrEmpty(scheduleDb.Cron))
                                {
                                    taskSchedule = new TaskCronSchedule(scheduleDb.Cron);
                                }

                                if (taskSchedule == null)
                                {
                                    continue;
                                }
                                taskSchedule.IsEnabled = scheduleDb.IsEnabled;
                                schedules.Add(taskSchedule);
                            }

                            var taskDescription = new TaskDescription
                            {
                                Id = taskDb.Id,
                                Name = taskDb.Name,
                                Description = taskDb.Description,
                                ExecutionLambda = null,
                                IsConfirmed = false,
                                UniqueKey = taskDb.UniqueKey,
                                IsEnabled = taskDb.IsEnabled ?? false,
                                TaskOptions = TaskOptions.None,
                                Schedules = new ReadOnlyCollection<TaskSchedule>(new List<TaskSchedule>()),
                                ManualSchedules = new ReadOnlyCollection<TaskSchedule>(schedules)
                            };
                            list[taskDb.Id] = taskDescription;
                        }
                    }
                }
                return list.Values.ToList();
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.CriticalError, "Ошибка во время получения списка задач", null, ex);
                throw new Exception("Неожиданная ошибка во время получения списка задач.");
            }
        }

        private void PrepareTaskSchedules(TaskDescription taskDescription)
        {
            foreach (var schedule in taskDescription.ManualSchedules)
            {
                var scheduleUniqueKey = schedule.GetUniqueKey();
                var uniqueKey = $"task_{taskDescription.UniqueKey}_sc_{scheduleUniqueKey}";

                if (!schedule.IsEnabled)
                {
                    lock (_jobsSyncRoot)
                    {
                        _jobsList.RemoveAll(x => x.JobName == uniqueKey);
                    }
                    continue;
                }

                if (schedule is TaskCronSchedule taskCronSchedule)
                {
                    SetTask(uniqueKey, taskCronSchedule.CronExpression, () => ExecuteTaskStatic(taskDescription.UniqueKey, scheduleUniqueKey));
                }
                else if (schedule is TaskFixedTimeSchedule taskFixedTimeSchedule)
                {
                    SetTask(uniqueKey, taskFixedTimeSchedule.DateTime, () => ExecuteTaskStatic(taskDescription.UniqueKey, scheduleUniqueKey));
                }
            }
            foreach (var schedule in taskDescription.Schedules)
            {
                var scheduleUniqueKey = schedule.GetUniqueKey();
                var uniqueKey = $"task_{taskDescription.UniqueKey}_sc_{scheduleUniqueKey}";

                if (schedule is TaskCronSchedule taskCronSchedule)
                {
                    SetTask(uniqueKey, taskCronSchedule.CronExpression, () => ExecuteTaskStatic(taskDescription.UniqueKey, scheduleUniqueKey));
                }
                else if (schedule is TaskFixedTimeSchedule taskFixedTimeSchedule)
                {
                    SetTask(uniqueKey, taskFixedTimeSchedule.DateTime, () => ExecuteTaskStatic(taskDescription.UniqueKey, scheduleUniqueKey));
                }
            }
        }

        /// <summary>
        /// Позволяет изменить состояние задачи.
        /// </summary>
        /// <param name="taskDescription">Зарегистрированная задача.</param>
        /// <param name="isEnabled">Новое состояние задачи.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="taskDescription"/> равен null.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если задача не зарегистрирована.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если для задачи запрещено изменение состояния (см. <see cref="TaskOptions.AllowDisabling"/>).</exception>
        public void SetTaskEnabled(TaskDescription taskDescription, bool isEnabled)
        {
            if (taskDescription == null) throw new ArgumentNullException(nameof(taskDescription));
            if (!_taskList.TryGetValue(taskDescription.UniqueKey, out var taskDescription2)) throw new InvalidOperationException("Неизвестная задача.");
            if (!taskDescription2.TaskOptions.HasFlag(TaskOptions.AllowDisabling)) throw new InvalidOperationException("Для задачи запрещено изменение состояния.");
            taskDescription2.IsEnabled = isEnabled;
            taskDescription.IsEnabled = isEnabled;
        }

        /// <summary>
        /// Позволяет изменить список дополнительных правил задачи.
        /// </summary>
        /// <param name="taskDescription">Зарегистрированная задача.</param>
        /// <param name="scheduleList">Список правил задачи.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="taskDescription"/> равен null.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если задача не зарегистрирована.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если для задачи запрещено изменение списка правил (см. <see cref="TaskOptions.AllowManualSchedule"/>).</exception>
        public void SetTaskManualScheduleList(TaskDescription taskDescription, List<TaskSchedule> scheduleList)
        {
            if (taskDescription == null) throw new ArgumentNullException(nameof(taskDescription));
            if (!_taskList.TryGetValue(taskDescription.UniqueKey, out var taskDescription2)) throw new InvalidOperationException("Неизвестная задача.");
            if (!taskDescription2.TaskOptions.HasFlag(TaskOptions.AllowManualSchedule)) throw new InvalidOperationException("Для задачи запрещено изменение списка правил.");
            if (scheduleList?.GroupBy(x => x.GetUniqueKey()).Any(x => x.Count() > 1) ?? false) throw new ArgumentException("В списке есть повторяющиеся правила запуска.", nameof(scheduleList));

            try
            {
                var schedules = scheduleList?.ToDictionary(x => x.GetUniqueKey(), x => x);
                var collection = new ReadOnlyCollection<TaskSchedule>(scheduleList ?? new List<TaskSchedule>());

                using (var db = new Db.DataContext())
                {
                    var list = db.TaskSchedule.Where(x => x.IdTask == taskDescription2.Id).ToList();

                    var listToRemove = list.Where(x => !schedules.ContainsKey(x.GetUniqueKey())).ToList();
                    db.TaskSchedule.RemoveRange(listToRemove);

                    var isChanged = listToRemove.Count > 0;

                    list.Where(x => schedules.ContainsKey(x.GetUniqueKey())).ForEach(x =>
                    {
                        isChanged = isChanged || x.IsEnabled != schedules[x.GetUniqueKey()].IsEnabled;
                        x.IsEnabled = schedules[x.GetUniqueKey()].IsEnabled;
                    });

                    schedules.ForEach(pair =>
                    {
                        if (!list.Any(x => x.GetUniqueKey() == pair.Key))
                        {
                            if (pair.Value is TaskCronSchedule taskCronSchedule)
                            {
                                isChanged = true;
                                db.TaskSchedule.Add(new Db.TaskSchedule()
                                {
                                    IdTask = taskDescription2.Id,
                                    IsEnabled = pair.Value.IsEnabled,
                                    Cron = taskCronSchedule.CronExpression
                                });
                            }
                            else if (pair.Value is TaskFixedTimeSchedule taskFixedTimeSchedule)
                            {
                                isChanged = true;
                                db.TaskSchedule.Add(new Db.TaskSchedule()
                                {
                                    IdTask = taskDescription2.Id,
                                    IsEnabled = pair.Value.IsEnabled,
                                    DateTimeFixed = taskFixedTimeSchedule.DateTime.UtcDateTime
                                });
                            }
                        }
                    });

                    if (isChanged) db.SaveChanges();

                    taskDescription2.ManualSchedules = collection;
                    taskDescription.ManualSchedules = collection;
                    PrepareTaskSchedules(taskDescription2);
                }
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.CriticalError, "Ошибка во время сохранения списка правил", null, ex);
                throw new Exception("Неожиданная ошибка во время сохранения списка правил.");
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
            _jobsTimer = new Timer(new TimerCallback(state => CheckTasks()), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
            if (_this == this) _this = null;

            lock (_jobsSyncRoot)
            {
                try { _jobsList.Clear(); }
                catch { }

                try
                {
                    var timer = _jobsTimer;
                    _jobsTimer = null;
                    timer?.Change(Timeout.Infinite, Timeout.Infinite);
                }
                catch { }
            }
        }

        private void CheckTasks()
        {
            lock (_jobsSyncRoot)
            {
                if (AppCore.GetState() != OnUtils.Architecture.AppCore.CoreComponentState.Started) return;

                var list = _delayedTaskRegistration;
                _delayedTaskRegistration = null;
                list?.ForEach(x => x());

                var offset = AppCore.Get<Modules.CoreModule.CoreModule>().ApplicationTimeZoneInfo.BaseUtcOffset;
                var appTime = DateTime.UtcNow.Add(offset);

                var jobsListSnapshot = _jobsList.Where(job => job.ClosestOccurrence <= appTime).ToList();
                _jobsList.RemoveAll(job => job.CronSchedule == null && job.ClosestOccurrence <= appTime);
                _jobsList.Where(job => job.CronSchedule != null).ForEach(job => job.ClosestOccurrence = job.CronSchedule.GetNextOccurrence(appTime));

                jobsListSnapshot.ForEach(job => Task.Factory.StartNew(() => job.ExecutionDelegate()));
            }
        }

        void SetTask(string name, string cronExpression, Expression<Action> taskDelegate)
        {
            lock (_jobsSyncRoot)
            {
                if (_jobsList.Any(x => x.JobName == name))
                    _jobsList.RemoveAll(x => x.JobName == name);

                var schedule = CrontabSchedule.Parse(cronExpression);
                var executionDelegate = taskDelegate.Compile();
                var baseTime = DateTime.UtcNow;

                if (_delayedTaskRegistration != null)
                {
                    _delayedTaskRegistration.Add(() => _jobsList.Add(new JobInternal()
                    {
                        CronSchedule = schedule,
                        JobName = name,
                        ExecutionDelegate = executionDelegate,
                        ClosestOccurrence = schedule.GetNextOccurrence(baseTime.Add(AppCore.Get<Modules.CoreModule.CoreModule>().ApplicationTimeZoneInfo.BaseUtcOffset))
                    }));
                }
                else
                {
                    _jobsList.Add(new JobInternal()
                    {
                        CronSchedule = schedule,
                        JobName = name,
                        ExecutionDelegate = executionDelegate,
                        ClosestOccurrence = schedule.GetNextOccurrence(baseTime.Add(AppCore.Get<Modules.CoreModule.CoreModule>().ApplicationTimeZoneInfo.BaseUtcOffset))
                    });
                }
            }
        }

        void SetTask(string name, DateTimeOffset startTime, Expression<Action> taskDelegate)
        {
            lock (_jobsSyncRoot)
            {
                if (_jobsList.Any(x => x.JobName == name))
                    _jobsList.RemoveAll(x => x.JobName == name);

                if (startTime < DateTimeOffset.Now) return;

                var executionDelegate = taskDelegate.Compile();

                if (_delayedTaskRegistration != null)
                {
                    _delayedTaskRegistration.Add(() => _jobsList.Add(new JobInternal()
                    {
                        CronSchedule = null,
                        JobName = name,
                        ExecutionDelegate = executionDelegate,
                        ClosestOccurrence = startTime.UtcDateTime.Add(AppCore.Get<Modules.CoreModule.CoreModule>().ApplicationTimeZoneInfo.BaseUtcOffset)
                    }));
                }
                else
                {
                    _jobsList.Add(new JobInternal()
                    {
                        CronSchedule = null,
                        JobName = name,
                        ExecutionDelegate = executionDelegate,
                        ClosestOccurrence = startTime.UtcDateTime.Add(AppCore.Get<Modules.CoreModule.CoreModule>().ApplicationTimeZoneInfo.BaseUtcOffset)
                    });
                }
            }
        }

        private void ExecuteTask(string taskUniqueKey, string scheduleUniqueKey)
        {
            if (!_taskList.TryGetValue(taskUniqueKey, out var taskDescription))
            {
                this.RegisterEvent(Journaling.EventType.Error, "Ошибка запуска", $"Задача не найдена. Ключ задачи - '{taskUniqueKey}'.");
                return;
            }

            var schedule = taskDescription.Schedules.Where(x => x.GetUniqueKey() == scheduleUniqueKey).FirstOrDefault();
            if (schedule == null)
            {
                schedule = taskDescription.ManualSchedules.Where(x => x.GetUniqueKey() == scheduleUniqueKey).FirstOrDefault();
                if (schedule == null)
                {
                    // 
                    return;
                }
                if (!schedule.IsEnabled)
                {
                    //
                    return;
                }
            }

            ExecuteTaskInternal(taskDescription);
        }

        private void ExecuteTaskInternal(TaskDescription taskDescription)
        {
            if (!_executeFlags.TryLock(taskDescription.UniqueKey) && taskDescription.TaskOptions.HasFlag(TaskOptions.PreventParallelExecution))
                return;

            try
            {
                this.RegisterEvent(Journaling.EventType.Info, "Запуск", $"Запуск задачи '{taskDescription.Name}' (№{taskDescription.Id} / '{taskDescription.UniqueKey}').");
                var timeStart = DateTime.Now;

                taskDescription.ExecutionLambda.Compile().Invoke();

                this.RegisterEvent(Journaling.EventType.Info, "Завершение", $"Задача '{taskDescription.Name}' (№{taskDescription.Id} / '{taskDescription.UniqueKey}') выполнена за {Math.Round((DateTime.Now - timeStart).TotalSeconds, 3)} сек.");
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.Info, "Ошибка выполнения", $"Неожиданная ошибка выполнения задачи '{taskDescription.Name}' (№{taskDescription.Id} / '{taskDescription.UniqueKey}').", ex);
            }
            finally
            {
                _executeFlags.ReleaseLock(taskDescription.UniqueKey);
            }
        }

        private static void ExecuteTaskStatic(string taskUniqueKey, string scheduleUniqueKey)
        {
            _this?.ExecuteTask(taskUniqueKey, scheduleUniqueKey);
        }

        /// <summary>
        /// Позволяет инициировать немедленный запуск задачи в отдельном объекте <see cref="System.Threading.Tasks.Task"/>.
        /// </summary>
        /// <param name="taskDescription">Запускаемая задача.</param>
        /// <return>Возвращает объект <see cref="System.Threading.Tasks.Task"/>.</return>
        public System.Threading.Tasks.Task ExecuteTask(TaskDescription taskDescription)
        {
            if (!_taskList.Any(x => x.Value == taskDescription)) throw new InvalidOperationException("Задача не зарегистрирована.");
            return System.Threading.Tasks.Task.Factory.StartNew(() => ExecuteTaskInternal(taskDescription));
        }
        #endregion
    }
}
