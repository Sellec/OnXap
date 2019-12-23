using System;

namespace OnXap.Modules.MessagingSMS
{
    using Messaging;

    /// <summary>
    /// Представляет сервис отправки СМС.
    /// </summary>
    public abstract class SMSService : MessageServiceBase<Message>, IDebugSupported
    {
        private string _debugRecipient;
        /// <summary>
        /// </summary>
        protected SMSService(string serviceName, Guid serviceID) : base(serviceName, serviceID)
        {
        }

        #region IDebugSupported
        /// <summary>
        /// Позволяет включить или отключить режим отладки. При включении этого компоненты, поддерживающие режим отладки, отправляют письма на адрес электронной почты, указанный в <paramref name="debugPhoneNumber"/>.
        /// </summary>
        /// <param name="isEnable">Признак включения или отключения режима отладки.</param>
        /// <param name="debugPhoneNumber">Адрес получателя сообщений для режима отладки. Не должен быть пустым, если <paramref name="isEnable"/> равен true.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="isEnable"/> равен true и <paramref name="debugPhoneNumber"/> пуст.</exception>
        public void SetDebugMode(bool isEnable, string debugPhoneNumber)
        {
            if (isEnable)
            {
                if (string.IsNullOrEmpty(debugPhoneNumber)) throw new ArgumentNullException(nameof(debugPhoneNumber));

                var phoneNumber = System.ComponentModel.DataAnnotations.PhoneBuilder.ParseString(debugPhoneNumber);
                if (!phoneNumber.IsCorrect) throw new ArgumentException(phoneNumber.Error, nameof(debugPhoneNumber));

                _debugRecipient = phoneNumber.ParsedPhoneNumber;
            }
            else
            {
                _debugRecipient = null;
            }
        }

        bool IDebugSupported.IsDebugModeEnabled()
        {
            return !string.IsNullOrEmpty(_debugRecipient);
        }

        string IDebugSupported.GetDebugRecipient()
        {
            return _debugRecipient;
        }
        #endregion

        /// <summary>
        /// Отправка смс-сообщения на номер телефона <paramref name="phoneTo"/> с текстом <paramref name="messageText"/>.
        /// </summary>
        /// <param name="phoneTo">Должен являться корректным номером телефона. В противном случае сгенерируется исключение <see cref="ArgumentException"/>.</param>
        /// <param name="messageText">Текст сообщения.</param>
        /// <returns>Возвращает результат постановки сообщения в очередь.</returns>
        public abstract void SendMessage(string phoneTo, string messageText);

        /// <summary>
        /// Отправка смс-сообщения на номер телефона администратора сайта текстом <paramref name="messageText"/>.
        /// </summary>
        /// <param name="messageText">Текст сообщения.</param>
        /// <returns>Возвращает результат постановки сообщения в очередь.</returns>
        public abstract void SendToAdmin(string messageText);
    }
}
