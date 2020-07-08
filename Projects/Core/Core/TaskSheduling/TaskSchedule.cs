using System;

namespace OnXap.TaskSheduling
{
    /// <summary>
    /// Описывает правило запуска задачи.
    /// </summary>
    public abstract class TaskSchedule
    {
        /// <summary>
        /// Возвращает уникальный ключ правила запуска.
        /// </summary>
        /// <returns></returns>
        public abstract string GetUniqueKey();

        /// <summary>
        /// Состояние правила - включено или выключено.
        /// </summary>
        /// <remarks>
        /// Все правила, заданные при регистрации задачи, считаются включенными, несмотря на переданное значение свойства.
        /// </remarks>
        public bool IsEnabled { get; set; }
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

        /// <summary>
        /// </summary>
        public sealed override string GetUniqueKey()
        {
            return CronExpression;
        }
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

        /// <summary>
        /// </summary>
        public sealed override string GetUniqueKey()
        {
            return DateTime.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
