using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace OnXap.TaskSheduling
{
    using Journaling;

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
        /// Позволяет определить или указать, разрешено ли выполнение задачи.
        /// </summary>
        /// <remarks>Если <see cref="TaskOptions"/> содержит флаг <see cref="TaskOptions.AllowDisabling"/> и задача была выключена, то изначальное значение свойства будет проигнорировано. 
        /// Если обязательно необходимо сменить статус задачи при её регистрации, следует вызвать метод <see cref="TaskSchedulingManager.SetTaskEnabled(TaskDescription, bool)"/>.</remarks>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Лямбда-выражение для выполнения задачи.
        /// </summary>
        public Expression<Action> ExecutionLambda { get; set; }

        /// <summary>
        /// Список правил запуска задачи.
        /// </summary>
        public List<TaskSchedule> Schedules { get; set; }

        /// <summary>
        /// Дополнительные параметры задачи.
        /// </summary>
        public TaskOptions TaskOptions { get; set; }

        /// <summary>
        /// Дополнительные параметры журналирования событий, связанных с задачей.
        /// </summary>
        public JournalOptions JournalOptions { get; set; }
    }
}
