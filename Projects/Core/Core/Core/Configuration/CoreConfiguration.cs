using Newtonsoft.Json;
using OnXap.Modules.CoreModule;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnXap.Core.Configuration
{
    /// <summary>
    /// Класс конфигурации. При создании экземпляра объекта через метод Create ядра <see cref="OnXApplication"/> автоматически заполняется значениями настроек ядра.
    /// </summary>
#pragma warning disable CS1591 // todo внести комментарии.
    public class CoreConfiguration : ModuleConfiguration<CoreModule>
    {
        /// <summary>
        /// Основной системный язык.
        /// </summary>
        [Display(Name = "Основной системный язык")]
        public int IdSystemLanguage
        {
            get => Get("IdSystemLanguage", -1);
            set => Set("IdSystemLanguage", value);
        }

        /// <summary>
        /// Настройки компонентов сервисов обработки сообщений.
        /// </summary>
        /// <seealso cref="Messaging.Components.MessageServiceComponent{TMessage}"/>
        /// <seealso cref="Messaging.MessagingManager"/>
        public List<Messaging.Components.ComponentSettings> MessageServicesComponentsSettings
        {
            get => JsonConvert.DeserializeObject<List<Messaging.Components.ComponentSettings>>(Get("MessageServicesComponentsSettings", "")) ?? new List<Messaging.Components.ComponentSettings>();
            set => Set("MessageServicesComponentsSettings", value == null ? "" : JsonConvert.SerializeObject(value));
        }

        /// <summary>
        /// Роль, присваиваемая контексту пользователя. Это позволяет задавать права по-умолчанию для всех пользователей.
        /// </summary>
        public int RoleUser
        {
            get => Get(Users.UserContextManager.RoleUserName, 0);
            set => Set(Users.UserContextManager.RoleUserName, value);
        }

        /// <summary>
        /// Роль, присваиваемая контексту гостя. Это позволяет задавать права по-умолчанию для всех гостей.
        /// </summary>
        public int RoleGuest
        {
            get => Get(Users.UserContextManager.RoleGuestName, 0);
            set => Set(Users.UserContextManager.RoleGuestName, value);
        }
    }
}
