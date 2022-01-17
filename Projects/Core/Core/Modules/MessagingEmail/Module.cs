namespace OnXap.Modules.MessagingEmail
{
    using Core.Modules;

    /// <summary>
    /// Модуль для управления настройками сервиса <see cref="EmailService"/>.
    /// </summary>
    [ModuleCore("EMail-сообщения")]
    public class Module : ModuleCore<Module>
    {
        private Components.SmtpServer _component = null;

        /// <summary>
        /// </summary>
        protected sealed internal override void OnModuleStarted()
        {
            OnConfigurationApplied();
        }

        /// <summary>
        /// </summary>
        protected sealed internal override void OnConfigurationApplied()
        {
            var cfg = GetConfiguration<ModuleConfiguration>();
            if (_component == null && cfg.IsUseSmtp)
            {
                _component = new Components.SmtpServer();
                AppCore.Get<Messaging.MessagingManager>().RegisterComponent(_component);
            }
            if (_component != null) _component.InitClient();
        }
    }
}
