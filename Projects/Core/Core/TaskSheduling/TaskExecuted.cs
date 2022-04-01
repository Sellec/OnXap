namespace OnXap.TaskSheduling
{
    /// <summary>
    /// Варианты завершения выполнения задачи.
    /// </summary>
    public enum TaskExecuted
    {
        /// <summary>
        /// Задача выполнена.
        /// </summary>
        Executed,

        /// <summary>
        /// Возникло необработанное исключение во время выполнения задачи.
        /// </summary>
        Faulted,

        /// <summary>
        /// Задача уже запущена, параллельное выполнение запрещено флагом <see cref="TaskOptions.PreventParallelExecution"/>.
        /// </summary>
        ParallelPrevented,

        /// <summary>
        /// Задача выключена.
        /// </summary>
        Disabled

    }
}
