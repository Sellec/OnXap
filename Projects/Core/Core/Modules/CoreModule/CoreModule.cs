using OnUtils.Architecture.AppCore;
using System;

namespace OnXap.Modules.CoreModule
{
    using Core.Modules;
    using Subscriptions;

    /// <summary>
    /// Интерфейс ядра системы для управления основными функциями.
    /// </summary>
    [ModuleCore("Ядро системы")]
    public sealed class CoreModule : ModuleCore<CoreModule>, ICritical
    {
        internal static readonly Guid PermissionConfigurationSave = "perm_configSave".GenerateGuid();
        private TimeZoneInfo _systemTimezone = TimeZoneInfo.Utc;
        internal SubscriptionDescription _subscriptionEventCritical = null;

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
            var subscriptionManager = AppCore.Get<SubscriptionsManager>();
            _subscriptionEventCritical = subscriptionManager.RegisterSubscription(
                "Журнал: уведомление о критических событиях",
                subscriptionManager.SubscriptionGroupSystem,
                new Journaling.CriticalJournalEventSubscription());

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
        /// Возвращает структуру <see cref="DateTimeOffset"/> с базовой датой-временем <paramref name="dateTime"/> в часовом поясе сервера.
        /// </summary>
        /// <remarks>Для <paramref name="dateTime"/> с <see cref="DateTime.Kind"/> равным <see cref="DateTimeKind.Unspecified"/> принимается, что передано <see cref="DateTimeKind.Utc"/>.</remarks>
        /// <seealso cref="Core.Configuration.CoreConfiguration.ApplicationTimezoneId"/>
        /// <seealso cref="ApplicationTimeZoneInfo"/>
        public DateTimeOffset ToServerDateTime(DateTime dateTime)
        {
            switch (dateTime.Kind)
            {
                case DateTimeKind.Utc:
                    return ToServerDateTime(DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified));

                case DateTimeKind.Unspecified:
                    return new DateTimeOffset(dateTime.Add(ApplicationTimeZoneInfo.BaseUtcOffset));

                case DateTimeKind.Local:
                    return ToServerDateTime(DateTime.SpecifyKind(dateTime.ToUniversalTime(), DateTimeKind.Unspecified));

                default:
                    throw new InvalidProgramException();
            }
        }

        /// <summary>
        /// Возвращает часовой пояс, в котором работает приложение.
        /// </summary>
        /// <seealso cref="Core.Configuration.CoreConfiguration.ApplicationTimezoneId"/>
        public TimeZoneInfo ApplicationTimeZoneInfo { get => _systemTimezone; }
    }
}
