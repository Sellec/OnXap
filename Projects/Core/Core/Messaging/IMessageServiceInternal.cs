namespace OnXap.Messaging
{
    using Core;

    interface IMessageServiceInternal : IComponentSingleton
    {
        void PrepareIncomingReceive();
        void PrepareIncomingHandle();
        void PrepareOutcoming();
    }
}
