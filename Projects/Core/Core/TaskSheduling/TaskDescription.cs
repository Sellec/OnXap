using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace OnXap.TaskSheduling
{
    using Journaling;

    /// <summary>
    /// Описание задачи.
    /// </summary>
    public class TaskDescription
    {
        internal TaskDescription()
        {
        }

        /// <summary>
        /// Идентификатор задачи.
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// Название задачи.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Описание задачи.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Уникальный ключ, позволяющий идентифицировать задачу.
        /// </summary>
        public string UniqueKey { get; internal set; }

        /// <summary>
        /// Равен true, если задача была подтверждена регистрацией. Равен false, если описание задачи получено из кеша без подтверждения регистрацией.
        /// </summary>
        /// <remarks>Неподтвержденная задача не может быть выполнена.</remarks>
        public bool IsConfirmed { get; internal set; }

        /// <summary>
        /// Указывает, разрешено ли выполнение задачи.
        /// </summary>
        public bool IsEnabled { get; internal set; }

        /// <summary>
        /// Дополнительные параметры задачи.
        /// </summary>
        public TaskOptions TaskOptions { get; internal set; }

        /// <summary>
        /// Лямбда-выражение для выполнения задачи.
        /// </summary>
        public Expression<Action> ExecutionLambda { get; internal set; }

        /// <summary>
        /// Список правил запуска задачи, присвоенных при регистрации.
        /// </summary>
        public ReadOnlyCollection<TaskSchedule> Schedules { get; internal set; }

        /// <summary>
        /// Список правил запуска задачи, созданных вручную.
        /// </summary>
        public ReadOnlyCollection<TaskSchedule> ManualSchedules { get; internal set; }

        /// <summary>
        /// Дополнительные параметры журналирования событий, связанных с задачей.
        /// </summary>
        public JournalOptions JournalOptions { get; set; }
    }
}
