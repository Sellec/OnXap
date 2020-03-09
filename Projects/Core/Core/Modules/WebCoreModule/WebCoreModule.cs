using OnUtils.Architecture.AppCore;
using OnUtils.Data;
using System;

namespace OnXap.Modules.WebCoreModule
{
    using Core.Modules;
    using Journaling;

    /// <summary>
    /// Интерфейс ядра системы для управления основными функциями.
    /// </summary>
    [ModuleCore("Ядро веб-системы")]
    public sealed class WebCoreModule : ModuleCore<WebCoreModule>, ICritical
    {
        internal static readonly Guid PermissionConfigurationSave = "perm_configSave".GenerateGuid();

        /// <summary>
        /// </summary>
        protected override void OnModuleStarting()
        {
            RegisterPermission(PermissionConfigurationSave, "Изменение настроек сайта.", "");
        }

        /// <summary>
        /// </summary>
        internal protected override void OnConfigurationApplied()
        {
            var hasSmsService = AppCore.Get<MessagingSMS.SMSService>() != null;
            if (!hasSmsService)
            {
                var cfg = GetConfigurationManipulator().GetEditable<WebCoreConfiguration>();
                switch (cfg.userAuthorizeAllowed)
                {
                    case Users.eUserAuthorizeAllowed.EmailAndPhone:
                    case Users.eUserAuthorizeAllowed.OnlyPhone:
                        cfg.userAuthorizeAllowed = Users.eUserAuthorizeAllowed.OnlyEmail;
                        this.RegisterEvent(EventType.CriticalError, "Сервис рассылки СМС не найден - режим авторизации сброшен", "Не найден сервис рассылки СМС-сообщений. В связи с этим режим авторизации пользователей изменен на 'Только Email'.");
                        GetConfigurationManipulator().ApplyConfiguration(cfg);
                        break;
                }
            }
        }

        internal void RunConfigurationCheck()
        {
            OnConfigurationApplied();
        }

    }
}
