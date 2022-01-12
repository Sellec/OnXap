namespace OnXap.Modules.Subscriptions
{
    using Messaging;

    /// <summary>
    /// </summary>
    public abstract class MessagingServiceSendAsUniversalConnectorInternal
    {
        internal void Send(SendAsUniversalInfo<IMessagingService> sendInfoUniversal)
        {
            OnSendInternal(sendInfoUniversal);
        }

        /// <summary>
        /// </summary>
        protected abstract void OnSendInternal(SendAsUniversalInfo<IMessagingService> sendInfoUniversal);
    }

    /// <summary>
    /// Коннектор для отправки универсальным способом, без прямого обращения к сервису сообщений.
    /// </summary>
    /// <typeparam name="TMessagingService"></typeparam>
    public abstract class MessagingServiceSendAsUniversalConnector<TMessagingService> : MessagingServiceSendAsUniversalConnectorInternal
        where TMessagingService : IMessagingService
    {
        /// <summary>
        /// </summary>
        protected sealed override void OnSendInternal(SendAsUniversalInfo<IMessagingService> sendInfoUniversal)
        {
            OnSend(new SendAsUniversalInfo<TMessagingService>()
            {
                Message = sendInfoUniversal.Message,
                MessagingService = (TMessagingService)sendInfoUniversal.MessagingService,
                SubscriptionDescription = sendInfoUniversal.SubscriptionDescription,
                UserIdList = sendInfoUniversal.UserIdList
            });
        }

        /// <summary>
        /// Вызывается для отправки сообщений.
        /// </summary>
        /// <param name="sendInfoUniversal">Информация об отправляемом сообщении.</param>
        protected abstract void OnSend(SendAsUniversalInfo<TMessagingService> sendInfoUniversal);
    }
}