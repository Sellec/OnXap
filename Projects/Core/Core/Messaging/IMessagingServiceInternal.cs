using System;

namespace OnXap.Messaging
{
    using Core;

    interface IMessagingServiceInternal : IComponentSingleton
    {
        void PrepareIncomingReceive(TimeSpan executeInterval);
        void PrepareIncomingHandle(TimeSpan executeInterval);
        void PrepareOutcoming(TimeSpan executeInterval);
    }
}
