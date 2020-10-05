namespace OnXap.Journaling
{
    /// <summary>
    /// Описывает дополнительные свойства журнала.
    /// </summary>
    public class JournalOptions
    {
        /// <summary>
        /// Ограничивает хранение записей в журнале за последние N дней.
        /// </summary>
        public int? LimitByLastNDays { get; set; }
    }
}
