using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace OnXap.TaskSheduling
{
    /// <summary>
    /// Описание задачи.
    /// </summary>
    public class TaskDescription
    {
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
        /// Указывает, разрешена ли донастройка расписания задачи.
        /// </summary>
        public bool AllowManualShedule { get; internal set; }

        /// <summary>
        /// Лямбда-выражение для выполнения задачи.
        /// </summary>
        public Expression<Action> ExecutionLambda { get; internal set; }

        /// <summary>
        /// Список правил запуска задачи.
        /// </summary>
        public List<TaskSchedule> Schedules { get; set; }
    }
}
