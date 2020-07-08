using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
