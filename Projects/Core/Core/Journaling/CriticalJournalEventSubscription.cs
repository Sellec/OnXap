namespace OnXap.Journaling
{
    using Modules.MessagingEmail;
    using Modules.Subscriptions;

    /// <summary>
    /// Подписка "Критическая ошибка в журналах".
    /// </summary>
    public class CriticalJournalEventSubscription : SubscriptionBase<CriticalJournalEventSubscription, CriticalJournalEventSubscription.Parameters>
    {
        /// <summary>
        /// Параметры подписки.
        /// </summary>
        public class Parameters
        {
            internal Parameters()
            {
            }

            /// <summary>
            /// Информация о журнале, в котором возникло событие.
            /// </summary>
            public DB.JournalNameDAO JournalInfo { get; set; }

            /// <summary>
            /// Информация о событии.
            /// </summary>
            public DB.JournalDAO EventInfo { get; set; }
        }

        internal CriticalJournalEventSubscription()
        {
        }

        /// <summary>
        /// </summary>
        protected override void OnRegister()
        {
            WithMessagingService<EmailService>(info =>
            {
                var body = "";
                body += $"Критическая ошибка в журнале '{info.Parameters.JournalInfo.Name}'.\r\n";
                body += $"Дата события: {info.Parameters.EventInfo.DateEvent.ToString("dd.MM.yyyy HH:mm:ss")}.\r\n";
                body += $"Сообщение: {info.Parameters.EventInfo.EventInfo}\r\n";
                if (!string.IsNullOrEmpty(info.Parameters.EventInfo.EventInfoDetailed)) body += $"Подробная информация: {info.Parameters.EventInfo.EventInfoDetailed}\r\n";
                if (!string.IsNullOrEmpty(info.Parameters.EventInfo.ExceptionDetailed)) body += $"Исключение: {info.Parameters.EventInfo.ExceptionDetailed}\r\n";

                info.MessagingContacts.FilterByMessagingService<EmailService>().
                    ForEach(x => info.MessagingService.SendMailFromSite(
                        x.Key.NameFull,
                        x.Value[0],
                        info.SubscriptionDescription.Name,
                        body,
                        ContentType.Text
                    ));
            });
        }
    }
}
