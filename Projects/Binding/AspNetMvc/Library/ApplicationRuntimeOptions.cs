using System;

namespace OnXap
{
    /// <summary>
    /// Описывает дополнительные настройки работы ASP.Net pipeline.
    /// </summary>
    [Flags]
    public enum ApplicationRuntimeOptions
    {
        /// <summary>
        /// </summary>
        None = 0,

        /// <summary>
        /// </summary>
        DebugLevelCommon = 1,

        /// <summary>
        /// </summary>
        DebugLevelDetailed = 2,
    }
}
