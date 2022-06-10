using System;

namespace OnXap.Messaging.Messages
{
    using Core.Items;

    /// <summary>
    /// Предоставляет информацию о сообщении.
    /// </summary>
    public class MessageInfo<TMessage> : ItemBase
        where TMessage : MessageBase
    {
        private int _idMessage = 0;

        internal MessageInfo(IntermediateStateMessage<TMessage> intermediateMessage)
        {
            _idMessage = intermediateMessage.MessageSource.IdQueue;
            Message = intermediateMessage.Message;
            State = intermediateMessage.MessageSource.State;
            switch(intermediateMessage.MessageSource.StateType)
            {
                case DB.MessageStateType.Complete:
                    StateType = MessageStateType.Completed;
                    break;

                case DB.MessageStateType.Error:
                    StateType = MessageStateType.Error;
                    break;

                case DB.MessageStateType.IntermediateAdded:
                    StateType = MessageStateType.NotHandled;
                    break;

                case DB.MessageStateType.NotProcessed:
                    StateType = intermediateMessage.MessageSource.DateDelayed.HasValue ? MessageStateType.Delayed : MessageStateType.NotHandled;
                    break;

                case DB.MessageStateType.Repeat:
                    StateType = MessageStateType.Repeat;
                    break;
            }
        }

        /// <summary>
        /// Создает новый экземпляр объекта.
        /// </summary>
        /// <param name="message">См. <see cref="Message"/>.</param>
        /// <param name="state">См. <see cref="State"/>.</param>
        public MessageInfo(TMessage message, string state = null)
        {
            Message = message;
            State = state;
        }

        /// <summary>
        /// Сообщение для отправки.
        /// </summary>
        public TMessage Message { get; }

        /// <summary>
        /// Дополнительное состояние сообщения. 
        /// Позволяет переносить дополнительную информацию о состоянии сообщения.
        /// </summary>
        public string State { get; }

        /// <summary>
        /// Тип состояния сообщения.
        /// </summary>
        public MessageStateType StateType { get; }

        #region ItemBase
        /// <summary>
        /// Возвращает идентификатор сообщения.
        /// </summary>
        public override int IdBase
        {
            get => _idMessage;
        }

        /// <summary>
        /// </summary>
        public override string CaptionBase
        {
            get => $"{IdBase}";
        }
        #endregion
    }

    /// <summary>
    /// Предоставляет информацию о принятом сообщении.
    /// </summary>
    public class ReceivedMessageInfo<TMessage> : MessageInfo<TMessage>
        where TMessage : MessageBase
    {
        /// <summary>
        /// Создает новый экземпляр принятого сообщения.
        /// </summary>
        /// <param name="message">См. <see cref="MessageInfo{TMessage}.Message"/>.</param>
        /// <param name="state">См. <see cref="MessageInfo{TMessage}.State"/>.</param>
        /// <param name="dateDelayed">Если задано, указывает время, после которого разрешено обрабатывать сообщение.</param>
        /// <param name="isComplete">Если равно true, то сообщение сразу помечается, как обработанное и не поступает в очередь для дальнейшей обработки в компонентах <see cref="Components.IncomingMessageHandler{TMessage}"/>.</param>
        /// <param name="isError">Если равно true, то сообщение отмечено как обработанное с ошибкой, state используется для передачи сообщения об ошибке.</param>
        public ReceivedMessageInfo(TMessage message, string state = null, DateTime? dateDelayed = null, bool isComplete = false, bool isError = false) : base(message, state)
        {
            dateDelayed = DateDelayed;
            IsComplete = isComplete;
        }

        /// <summary>
        /// Если задано, указывает время, после которого разрешено обрабатывать сообщение.
        /// </summary>
        public DateTime? DateDelayed { get; }

        /// <summary>
        /// Если равно true, то сообщение не поступает в очередь для дальнейшей обработки в компонентах <see cref="Components.IncomingMessageHandler{TMessage}"/>.
        /// </summary>
        public bool IsComplete { get; }

        /// <summary>
        /// Если равно true, то сообщение отмечено как обработанное с ошибкой, state используется для передачи сообщения об ошибке.
        /// </summary>
        public bool IsError { get; }
    }
}
