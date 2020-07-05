using System;

namespace OnXap.TaskSheduling
{
    /// <summary>
    /// Описывает правило запуска задачи.
    /// </summary>
    public abstract class TaskSchedule
    {

    }

    /// <summary>
    /// Запуск задачи по правилу в стиле планировщика Cron.
    /// </summary>
    public class TaskCronSchedule : TaskSchedule
    {
        /// <summary>
        /// Создает новый экземпляр объекта с указанным правилом запуска в формате Cron.
        /// </summary>
        public TaskCronSchedule(string cronExpression)
        {
            CronExpression = cronExpression;
        }

        /// <summary>
        /// Правило запуска.
        /// </summary>
        public string CronExpression { get; }
    }

    /// <summary>
    /// Запуск задачи в указанную дату и время.
    /// </summary>
    public class TaskFixedTimeSchedule : TaskSchedule
    {
        /// <summary>
        /// Создает новый экземпляр объекта с указанными датой и временем запуска.
        /// </summary>
        public TaskFixedTimeSchedule(DateTimeOffset dateTime)
        {
            DateTime = dateTime;
        }

        /// <summary>
        /// Дата и время запуска.
        /// </summary>
        public DateTimeOffset DateTime { get; }
    }
}
