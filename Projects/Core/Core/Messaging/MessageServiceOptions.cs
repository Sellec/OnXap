namespace OnXap.Messaging
{
    /// <summary>
    /// Описывает дополнительные свойства сервиса обработки сообщений.
    /// </summary>
    public class MessageServiceOptions
    {
        /// <summary>
        /// Ограничивает хранение сообщений за последние N дней.
        /// </summary>
        public int? LimitByLastNDays { get; set; }
    }
}
