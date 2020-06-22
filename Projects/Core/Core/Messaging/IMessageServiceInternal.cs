using System;

namespace OnXap.Messaging
{
    using Core;

    interface IMessageServiceInternal : IComponentSingleton
    {
        void PrepareIncomingReceive(TimeSpan executeInterval);
        void PrepareIncomingHandle(TimeSpan executeInterval);
        void PrepareOutcoming(TimeSpan executeInterval);
    }
}
