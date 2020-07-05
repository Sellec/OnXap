using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace OnXap.TaskSheduling
{
    /// <summary>
    /// Запрос для регистрации задачи.
    /// </summary>
    public class TaskRequest
    {
        /// <summary>
        /// Название задачи.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание задачи.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Уникальный ключ, позволяющий идентифицировать задачу.
        /// </summary>
        public string UniqueKey { get; set; }

        /// <summary>
        /// Указывает, разрешена ли донастройка расписания задачи.
        /// </summary>
        public bool AllowManualShedule { get; set; }

        /// <summary>
        /// Лямбда-выражение для выполнения задачи.
        /// </summary>
        public Expression<Action> ExecutionLambda { get; set; }

        /// <summary>
        /// Список правил запуска задачи.
        /// </summary>
        public List<TaskSchedule> Schedules { get; set; }

    }
}
