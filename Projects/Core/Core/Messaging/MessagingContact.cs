using System;
using System.Collections.Generic;
using System.Linq;

namespace OnXap.Messaging
{
    /// <summary>
    /// Представляет список контактов со вспомогательными методами доступа к данным.
    /// </summary>
    public class MessagingContacts : List<MessagingContact>
    {
        /// <summary>
        /// Возвращает список контактов, для которых найдены контактные данные для конкретного сервиса обмена сообщениями <typeparamref name="TMessagingService"/>.
        /// </summary>
        public Dictionary<MessagingContact, List<string>> FilterByMessagingService<TMessagingService>()
            where TMessagingService : class, IMessagingService
        {
            return this.
                Select(x => new { x, data = x.GetData<TMessagingService>() }).
                Where(x => x.data != null).
                ToDictionary(x => x.x, x => x.data);
        }
    }

    /// <summary>
    /// Представляет данные о контакте для систем обмена сообщениями.
    /// </summary>
    public class MessagingContact
    {
        internal Dictionary<Type, List<string>> _data = new Dictionary<Type, List<string>>();

        /// <summary>
        /// Идентификатор контакта.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Полное имя контакта.
        /// </summary>
        public string NameFull { get; set; }

        /// <summary>
        /// Возвращает контактные данные для конкретного сервиса обмена сообщениями <typeparamref name="TMessagingService"/> или null, если ничего не найдено.
        /// </summary>
        public List<string> GetData<TMessagingService>()
            where TMessagingService : class, IMessagingService
        {
            return _data.TryGetValue(typeof(TMessagingService), out var value) ? value : null;
        }

        /// <summary>
        /// Позволяет определить, найдены ли контактные данные для конкретного сервиса обмена сообщениями <typeparamref name="TMessagingService"/>.
        /// </summary>
        public bool HasData<TMessagingService>() where TMessagingService : class, IMessagingService
        {
            return _data.ContainsKey(typeof(TMessagingService));
        }

        /// <summary>
        /// Задает контактные данные для конкретного сервиса обмена сообщениями <typeparamref name="TMessagingService"/>.
        /// </summary>
        public void SetData<TMessagingService>(List<string> data)
            where TMessagingService : class, IMessagingService
        {
            _data[typeof(TMessagingService)] = data;
        }

    }
}
