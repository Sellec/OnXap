using System.ComponentModel.DataAnnotations;

namespace OnXap.Modules.MessagingEmail.Model
{
    public class Configuration : Core.Modules.Configuration.SaveModel
    {
        [Display(Name = "Использовать отдельный SMTP-сервер для отправки почты?")]
        public bool IsUseSmtp { get; set; }

        /// <summary>
        /// См. <see cref="ModuleConfiguration.OutgoingAddress"/>
        /// </summary>
        [Display(Name = "Обратный адрес для писем")]
        [DataType(DataType.EmailAddress)]
        [MaxLength(200)]
        public string OutgoingAddress { get; set; }

        /// <summary>
        /// См. <see cref="ModuleConfiguration.OutgoingName"/>
        /// </summary>
        [Display(Name = "Отображаемое имя для обратного адреса")]
        [MaxLength(200)]
        public string OutgoingName { get; set; }

        /// <summary>
        /// См. <see cref="ModuleConfiguration.Server"/>
        /// </summary>
        [Display(Name = "Адрес smtp-сервера")]
        [Required]
        [MaxLength(200)]
        public string Server { get; set; }

        /// <summary>
        /// См. <see cref="ModuleConfiguration.IsSecure"/>
        /// </summary>
        [Display(Name = "Использовать подключение по SSL?")]
        public bool IsSecure { get; set; }

        /// <summary>
        /// См. <see cref="ModuleConfiguration.IsIgnoreCertificateErrors"/>
        /// </summary>
        [Display(Name = "Игнорировать ошибки сертификата?")]
        public bool IsIgnoreCertificateErrors { get; set; }

        /// <summary>
        /// См. <see cref="ModuleConfiguration.Port"/>
        /// </summary>
        [Display(Name = "Порт подключения (необязателен для заполнения. По-умолчанию для небезопасного подключения используется порт 80, для безопасного 587)")]
        public int? Port { get; set; }

        /// <summary>
        /// </summary>
        [Display(Name = "Анонимная авторизация")]
        public bool IsAnonymous { get; set; }

        /// <summary>
        /// См. <see cref="ModuleConfiguration.Login"/>
        /// </summary>
        [Display(Name = "Логин для подключения к smtp-серверу")]
        [MaxLength(200)]
        [DataType(DataType.Text)]
        public string Login { get; set; }

        /// <summary>
        /// См. <see cref="ModuleConfiguration.Password"/>
        /// </summary>
        [Display(Name = "Пароль для подключения к smtp-серверу")]
        [MaxLength(200)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public void ApplyConfiguration(ModuleConfiguration cfg)
        {
            IsUseSmtp = cfg.IsUseSmtp;
            IsAnonymous = string.IsNullOrEmpty(cfg.Login);
            OutgoingAddress = cfg.OutgoingAddress;
            OutgoingName = cfg.OutgoingName;
            Server = cfg.Server;
            IsSecure = cfg.IsSecure;
            IsIgnoreCertificateErrors = cfg.IsIgnoreCertificateErrors;
            Port = cfg.Port;
            Login = cfg.Login;
            Password = cfg.Password;
        }
    }
}