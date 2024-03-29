﻿using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace OnXap.Modules.WebCoreModule
{
    using Core.Configuration;
    using Users;
    using Types;

#pragma warning disable CS1591 // todo внести комментарии.
    /// <summary>
    /// Класс конфигурации. При создании экземпляра объекта через метод Create ядра <see cref="OnXApplication"/> автоматически заполняется значениями настроек ядра.
    /// </summary>
    public class WebCoreConfiguration : ModuleConfiguration<WebCoreModule>
    {
        [Display(Name = "Название сайта"), Required(ErrorMessage = "Название сайта не может быть пустым"), MaxLength(200)]
        public string SiteFullName
        {
            get => Get("site_title", "");
            set => Set("site_title", value);
        }

        [Display(Name = "Модуль, загружаемый по-умолчанию"), ModuleValidation(ErrorMessage = "Модуль, открывающийся по-умолчанию, должен быть выбран.")]
        public int IdModuleDefault
        {
            get => Get("index_module", 0);
            set => Set("index_module", value);
        }

        [Display(Name = "Email для отправки сообщений о критических ошибках"), Required(ErrorMessage = "Email для отправки сообщений о критических ошибках не может быть пустым")]
        [EmailAddress(ErrorMessage = "Неправильно указан email для отправки сообщений о критических ошибках")]
        public string CriticalMessagesEmail
        {
            get => Get("CriticalMessagesEmail", "");
            set => Set("CriticalMessagesEmail", value);
        }

        [Display(Name = "Режим регистрации на сайте")]
        public RegisterMode register_mode
        {
            get => Get("register_mode", RegisterMode.Immediately);
            set => Set("register_mode", value);
        }

        [Display(Name = "Режим авторизации на сайте")]
        public eUserAuthorizeAllowed userAuthorizeAllowed
        {
            get => Get("userAuthorizeAllowed", eUserAuthorizeAllowed.OnlyEmail);
            set => Set("userAuthorizeAllowed", value);
        }

        [Display(Name = "Информация перед регистрацией")]
        public string site_reginfo
        {
            get => Get("site_reginfo", "");
            set => Set("site_reginfo", value);
        }

        [Display(Name = "Информация после входа в систему")]
        public string site_loginfo
        {
            get => Get("site_loginfo", "");
            set => Set("site_loginfo", value);
        }

        [Display(Name = "Контактная информация (на странице обратной связи)")]
        public string help_info
        {
            get => Get("help_info", "");
            set => Set("help_info", value);
        }

        [Display(Name = "SEO Описание сайта(description)")]
        public string site_descr
        {
            get => Get("site_descr", "");
            set => Set("site_descr", value);
        }

        [Display(Name = "SEO Ключевые слова (keywords)")]
        public string site_keys
        {
            get => Get("site_keys", "");
            set => Set("site_keys", value);
        }

        /// <summary>
        /// Email разработчика сайта, который должен получать уведомления, связанные с отслеживанием работы кода.
        /// </summary>
        [Display(Name = "Email разработчика сайта")]
        public string DeveloperEmail
        {
            get => Get("DeveloperEmail", "");
            set => Set("DeveloperEmail", value);
        }
    }
}
