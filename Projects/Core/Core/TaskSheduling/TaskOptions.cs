using System;

namespace OnXap.TaskSheduling
{
    /// <summary>
    /// Описывает дополнительные параметры задачи.
    /// </summary>
    [Flags]
    public enum TaskOptions : int
    {
        /// <summary>
        /// </summary>
        None = 0,

        /// <summary>
        /// Разрешена донастройка расписания запуска.
        /// </summary>
        AllowManualSchedule = 1,

        /// <summary>
        /// Разрешено включение/выключение задачи.
        /// </summary>
        AllowDisabling = 2,

        /// <summary>
        /// Предотвращать выполнение одной задачи в параллельных потоках.
        /// </summary>
        PreventParallelExecution = 4,
    }
}
