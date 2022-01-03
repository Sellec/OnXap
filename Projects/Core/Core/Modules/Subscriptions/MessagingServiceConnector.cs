using System.Collections.Generic;

namespace OnXap.Modules.Subscriptions
{
    using Messaging;

    public abstract class MessagingServiceConnectorInternal
    {
        internal void SendUniversal(SendInfoUniversal<IMessageService> sendInfoUniversal)
        {
            OnSendUniversalInternal(sendInfoUniversal);
        }

        protected abstract void OnSendUniversalInternal(SendInfoUniversal<IMessageService> sendInfoUniversal);

        //internal void SendServiceSpecified(List<int> userIdList, object message, IMessageService messagingService)
        //{
        //    OnSendServiceSpecifiedInternal(userIdList, message, messagingService);
        //}

        //protected abstract void OnSendServiceSpecifiedInternal(List<int> userIdList, object message, IMessageService messagingService);
    }

    public abstract class MessagingServiceConnector<TMessagingService> : MessagingServiceConnectorInternal
        where TMessagingService : IMessageService
    {
        protected sealed override void OnSendUniversalInternal(SendInfoUniversal<IMessageService> sendInfoUniversal)
        {
            OnSendUniversal(new SendInfoUniversal<TMessagingService>()
            {
                Message = sendInfoUniversal.Message,
                MessagingService = (TMessagingService)sendInfoUniversal.MessagingService,
                SubscriptionDescription = sendInfoUniversal.SubscriptionDescription,
                UserIdList = sendInfoUniversal.UserIdList
            });
        }

        protected abstract void OnSendUniversal(SendInfoUniversal<TMessagingService> sendInfoUniversal);

        //protected sealed override void OnSendServiceSpecifiedInternal(List<int> userIdList, object message, IMessageService messagingService)
        //{
        //    OnSendServiceSpecified(userIdList, message, (TMessagingService)messagingService);
        //}

        //protected abstract void OnSendServiceSpecified(List<int> userIdList, object message, TMessagingService messagingService);
    }
}