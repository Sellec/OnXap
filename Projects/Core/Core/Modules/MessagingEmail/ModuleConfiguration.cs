namespace OnXap.Modules.MessagingEmail
{
    using Core.Configuration;

    /// <summary>
    /// Хранит настройки подключения к smtp-серверу.
    /// </summary>
    public class ModuleConfiguration : ModuleConfiguration<Module>
    {
        /// <summary>
        /// Указывает, включен ли smtp-сервис для отправки и получения писем.
        /// </summary>
        public bool IsUseSmtp
        {
            get => Get(nameof(IsUseSmtp), false);
            set => Set(nameof(IsUseSmtp), value);
        }

        /// <summary>
        /// Обратный адрес для писем. Если не указан, то используется динамически генерируемый адрес, см. <see cref="EmailService.GetDefaultOutgoingAddress"/>.
        /// </summary>
        public string OutgoingAddress
        {
            get => Get(nameof(OutgoingAddress), string.Empty);
            set => Set(nameof(OutgoingAddress), value);
        }

        /// <summary>
        /// Отображаемое имя для обратного адреса. Может быть пустым.
        /// </summary>
        public string OutgoingName
        {
            get => Get(nameof(OutgoingName), string.Empty);
            set => Set(nameof(OutgoingName), value);
        }

        /// <summary>
        /// Адрес сервера.
        /// </summary>
        public string Server
        {
            get => Get(nameof(Server), string.Empty);
            set => Set(nameof(Server), value);
        }

        /// <summary>
        /// Указывает, следует ли использовать SSL.
        /// </summary>
        public bool IsSecure
        {
            get => Get(nameof(IsSecure), false);
            set => Set(nameof(IsSecure), value);
        }

        /// <summary>
        /// Указывает, следует ли игнорировать ошибки сертификата.
        /// </summary>
        public bool IsIgnoreCertificateErrors
        {
            get => Get(nameof(IsIgnoreCertificateErrors), true);
            set => Set(nameof(IsIgnoreCertificateErrors), value);
        }

        /// <summary>
        /// Порт подключения. Если не задан, то при <see cref="IsSecure"/> равном false используется порт 80, при <see cref="IsSecure"/> равном true используется порт 587.
        /// </summary>
        public int? Port
        {
            get => Get(nameof(Port), (int?)null);
            set => Set(nameof(Port), value);
        }

        /// <summary>
        /// Логин для подключения к smtp-серверу. Если не задан, то используется анонимный метод авторизации на сервере.
        /// </summary>
        public string Login
        {
            get => Get(nameof(Login), string.Empty);
            set => Set(nameof(Login), value);
        }

        /// <summary>
        /// Пароль для подключения к smtp-серверу.
        /// </summary>
        public string Password
        {
            get => Get(nameof(Password), string.Empty);
            set => Set(nameof(Password), value);
        }
    }
}
