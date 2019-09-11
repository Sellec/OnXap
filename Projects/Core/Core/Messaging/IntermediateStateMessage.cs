namespace OnXap.Messaging
{
    using Messages;

    class IntermediateStateMessage<TMessageType> where TMessageType : MessageBase
    {
        internal IntermediateStateMessage(TMessageType message, DB.MessageQueue messageSource)
        {
            Message = message;
            MessageSource = messageSource;
        }

        public DB.MessageQueue MessageSource
        {
            get;
        }

        public TMessageType Message
        {
            get;
        }
    }
}
