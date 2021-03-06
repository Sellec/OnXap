﻿using System.ComponentModel.DataAnnotations;

namespace OnXap.ServiceMonitor
{
    /// <summary>
    /// Перечисление основных состояний сервиса.
    /// </summary>
    public enum ServiceStatus
    {
        /// <summary>
        /// Не запущен.
        /// </summary>
        [Display(Name = "Не запущен")]
        Shutdown,

        /// <summary>
        /// Работает без ошибок.
        /// </summary>
        [Display(Name = "Работает без ошибок")]
        RunningIdeal,

        /// <summary>
        /// Работает с ошибками.
        /// </summary>
        [Display(Name = "Работает с ошибками")]
        RunningWithErrors,

        /// <summary>
        /// Не может работать из-за критических ошибок.
        /// </summary>
        [Display(Name = "Не может работать из-за критических ошибок")]
        CannotRunBecouseOfErrors,
    }
}
