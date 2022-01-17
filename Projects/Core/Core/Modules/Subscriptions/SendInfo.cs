namespace OnXap.Modules.Subscriptions
{
    using Messaging;

    /// <summary>
    /// Информация о событии подписки.
    /// </summary>
    /// <typeparam name="TMessagingService"></typeparam>
    /// <typeparam name="TParameters"></typeparam>
    public struct SendInfo<TMessagingService, TParameters>
        where TMessagingService : IMessagingService
    {
        /// <summary>
        /// Список контактов-получателей сообщения.
        /// </summary>
        public MessagingContacts MessagingContacts { get; set; }

        /// <summary>
        /// Сервис-отправитель сообщения.
        /// </summary>
        public TMessagingService MessagingService { get; set; }

        /// <summary>
        /// Параметры вызова для подписки.
        /// </summary>
        public TParameters Parameters { get; set; }

        /// <summary>
        /// Информация о подписке, в рамках которой происходит отправка сообщения.
        /// </summary>
        public SubscriptionDescription SubscriptionDescription { get; set; }
    }


}
