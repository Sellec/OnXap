namespace OnXap.Journaling.Model
{
    using Core.DB;
    using Core.Items;
    using Users;
    using System;

    /// <summary>
    /// Представляет запись в журнале.
    /// </summary>
    public class JournalData
    {
        internal static void Fill(JournalData dest, DB.JournalDAO source, JournalInfo journal, UserInfo user)
        {
            dest.IdJournalData = source.IdJournalData;
            dest.JournalInfo = journal;
            dest.EventType = source.EventType;
            dest.EventCode = source.EventCode;
            dest.EventInfo = source.EventInfo;
            dest.EventInfoDetailed = source.EventInfoDetailed;
            dest.ExceptionDetailed = source.ExceptionDetailed;
            dest.DateEvent = source.DateEvent;
            dest.User = user;
            dest.ItemLinkId = source.ItemLinkId;
        }

        /// <summary>
        /// Идентификатор записи журнала.
        /// </summary>
        public int IdJournalData { get; private set; }

        /// <summary>
        /// Информация о журнале, к которому относятся записи.
        /// </summary>
        public JournalInfo JournalInfo { get; private set; }

        /// <summary>
        /// Тип события.
        /// </summary>
        public EventType EventType { get; private set; }

        /// <summary>
        /// Код события.
        /// </summary>
        public int EventCode { get; private set; }

        /// <summary>
        /// Основная информация о событии.
        /// </summary>
        public string EventInfo { get; private set; }

        /// <summary>
        /// Детализированная информация о событии.
        /// </summary>
        public string EventInfoDetailed { get; private set; }

        /// <summary>
        /// Информация об исключении, если событие сопровождалось возникновением исключения.
        /// </summary>
        public string ExceptionDetailed { get; private set; }

        /// <summary>
        /// Дата фиксации события.
        /// </summary>
        public DateTime DateEvent { get; private set; }

        /// <summary>
        /// Информация о пользователе, создавшем запись.
        /// </summary>
        public UserInfo User { get; private set; }

        /// <summary>
        /// Идентификатор ссылки на объект, с которым связано событие.
        /// </summary>
        /// <seealso cref="ItemKey"/>.
        public Guid? ItemLinkId { get; private set; }
    }
}
