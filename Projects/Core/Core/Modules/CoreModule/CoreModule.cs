using OnUtils.Architecture.AppCore;
using System;

namespace OnXap.Modules.CoreModule
{
    using Core.Modules;

    /// <summary>
    /// Интерфейс ядра системы для управления основными функциями.
    /// </summary>
    [ModuleCore("Ядро системы")]
    public sealed class CoreModule : ModuleCore<CoreModule>, ICritical
    {
        internal static readonly Guid PermissionConfigurationSave = "perm_configSave".GenerateGuid();
        private TimeZoneInfo _systemTimezone = TimeZoneInfo.Utc;

        /// <summary>
        /// </summary>
        protected override void OnModuleStarting()
        {
            RegisterPermission(PermissionConfigurationSave, "Изменение настроек приложения", "");
        }

        /// <summary>
        /// </summary>
        protected internal override void OnModuleStarted()
        {
            OnConfigurationApplied();
        }

        /// <summary>
        /// </summary>
        protected internal override void OnConfigurationApplied()
        {
            var cfg = GetConfiguration<Core.Configuration.CoreConfiguration>();

            try
            {
                _systemTimezone = TimeZoneInfo.FindSystemTimeZoneById(cfg.ApplicationTimezoneId);
            }
            catch (Exception)
            {
                _systemTimezone = TimeZoneInfo.Utc;
            }
        }

        /// <summary>
        /// Возвращает часовой пояс, в котором работает приложение.
        /// </summary>
        /// <seealso cref="Core.Configuration.CoreConfiguration.ApplicationTimezoneId"/>
        public TimeZoneInfo ApplicationTimeZoneInfo { get => _systemTimezone; }
    }
}
