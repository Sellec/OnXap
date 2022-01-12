using System.Collections.Generic;

namespace OnXap.Modules.Subscriptions
{
    using Messaging;

    /// <summary>
    /// Информация о событии подписки.
    /// </summary>
    /// <typeparam name="TMessagingService"></typeparam>
    public struct SendInfo<TMessagingService>
        where TMessagingService : IMessagingService
    {
        /// <summary>
        /// Список идентификаторов пользователей-получателей сообщения.
        /// </summary>
        public List<int> UserIdList { get; set; }

        /// <summary>
        /// Сервис-отправитель сообщения.
        /// </summary>
        public TMessagingService MessagingService { get; set; }

        /// <summary>
        /// Информация о подписке, в рамках которой происходит отправка сообщения.
        /// </summary>
        public SubscriptionDescription SubscriptionDescription { get; set; }
    }


}
