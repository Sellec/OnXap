using OnXap.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnXap.Modules.MessagingEmail
{
    using Core.Items;
    using Messaging;
    using Messaging.DB;

    /// <summary>
    /// Представляет сервис отправки электронных писем (Email).
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class EmailService : MessageServiceBase<EmailMessage>, ICriticalMessagesReceiver, IDebugSupported
    {
        private bool _debugIsEnabled = false;
        private string _debugRecipient = null;

        /// <summary>
        /// </summary>
        public EmailService() : base("Email", "Email".GenerateGuid())
        {
            IsSupportsIncoming = false;
            IsSupportsOutcoming = true;
            IsSupportsCurrentStatusInfo = false;
        }

        #region IDebugSupported
        /// <summary>
        /// Позволяет включить или отключить режим отладки. При включении этого компоненты, поддерживающие режим отладки, отправляют письма на адрес электронной почты, указанный в <paramref name="debugEmail"/>.
        /// </summary>
        /// <param name="isEnable">Признак включения или отключения режима отладки.</param>
        /// <param name="debugEmail">Адрес получателя сообщений для режима отладки. Если задано пустое значение, то сообщения не отправляются до тех пор, пока режим отладки не будет отключен либо включен с указанным значением адреса.</param>
        public void SetDebugMode(bool isEnable, string debugEmail)
        {
            _debugIsEnabled = isEnable;
            _debugRecipient = isEnable ? debugEmail : null;
        }

        bool IDebugSupported.IsDebugModeEnabled()
        {
            return _debugIsEnabled;
        }

        string IDebugSupported.GetDebugRecipient()
        {
            return _debugRecipient;
        }
        #endregion

        #region Отправка
        /// <summary>
        /// Отправка письма на указанный адрес, с указанной темой, с указанным текстом.
        /// </summary>
        /// <param name="name_from">Имя отправителя</param>
        /// <param name="email_from">email отправителя</param>
        /// <param name="name_to">имя получателя</param>
        /// <param name="email_to">email получателя</param>
        /// <param name="data_charset">кодировка переданных данных</param>
        /// <param name="send_charset">кодировка письма</param>
        /// <param name="subject">тема письма</param>
        /// <param name="body">текст письма</param>
        /// <param name="files">Прикрепленные файлы</param>
        /// <returns></returns>
        public void SendMail(string name_from, string email_from, string name_to, string email_to, Encoding data_charset, Encoding send_charset, string subject, string body, ContentType contentType, List<int> files = null)
        {
            if (contentType == ContentType.Text) body = body.Replace("\n", "\n<br />");

            var message = new EmailMessage()
            {
                From = new Contact<string>(name_from, email_from),
                To = new List<Contact<string>>() { new Contact<string>(name_to, email_to) },
                Subject = subject,
                Body = body,
            };

            RegisterOutcomingMessage(message, out var messageInfo);
        }

        /// <summary>
        /// Отправка письма получателю <paramref name="nameTo"/> с адресом <paramref name="emailTo"/> с темой <paramref name="subject"/>, с текстом <paramref name="body"/>.
        /// </summary>
        public void SendMailFromSite(string nameTo, string emailTo, string subject, string body, ContentType contentType, List<int> files = null)
        {
            SendMail("Почтовый робот сайта", GetNoReplyAddress(), nameTo, emailTo, null, null, subject, body, contentType, files);
        }

        public void SendMailToDeveloper(string subject, string body, ContentType contentType, List<int> files = null)
        {
            SendMail("Почтовый робот сайта", GetNoReplyAddress(), AppCore.WebConfig.DeveloperEmail, AppCore.WebConfig.DeveloperEmail, null, null, subject, body, contentType, files);
        }

        /// <summary>
        /// Производит рассылку письма всем подписчикам указанной подписки.
        /// </summary>
        /// <param name="idSubscription">Идентификатор подписки.</param>
        /// <param name="subject">Тема письма.</param>
        /// <param name="body">Тело письма.</param>
        /// <param name="contentType">Тип содержимого письма.</param>
        /// <param name="availableUserStates">Письма будут отправлены пользователям с указанными состояниями учетных записей. Если параметр не задан, то в качестве значения по-умолчанию используется <see cref="Core.Db.UserState.Active"/>.</param>
        public void SendMailSubscription(int idSubscription, string subject, string body, ContentType contentType, Core.Db.UserState[] availableUserStates = null)
        {
            var itemKey = new ItemKey(ItemTypeFactory.GetItemType<MessageSubscription>().IdItemType, idSubscription);

            try
            {
                using (var db = new DataContext())
                {
                    var subscription = db.MessageSubscription.Where(x => x.IdSubscription == idSubscription).FirstOrDefault();
                    if (subscription == null)
                    {
                        this.RegisterEventForItem(itemKey, Journaling.EventType.Error, 0, "Ошибка рассылки электронной почты", $"Рассылка №{idSubscription} не найдена.");
                        return;
                    }

                    if (!subscription.IsEnabled)
                    {
                        this.RegisterEventForItem(itemKey, Journaling.EventType.Warning, 0, "Попытка рассылки электронной почты по неактивной рассылке", $"Рассылка №{idSubscription} неактивна.");
                        return;
                    }

                    var availableUserStatesInternal = availableUserStates?.Distinct().ToArray() ?? new Core.Db.UserState[] { Core.Db.UserState.Active };

                    var query = from MessageSubscriptionRole in db.MessageSubscriptionRole
                                join Role in db.Role on MessageSubscriptionRole.IdRole equals Role.IdRole
                                join RoleUser in db.RoleUser on Role.IdRole equals RoleUser.IdRole
                                join User in db.User on RoleUser.IdUser equals User.IdUser
                                where MessageSubscriptionRole.IdSubscription == idSubscription && User.Block == 0 && availableUserStatesInternal.Contains(User.State) && !string.IsNullOrEmpty(User.email)
                                select new
                                {
                                    User.name,
                                    User.email
                                };

                    var list = query.ToList();
                    foreach (var user in list)
                    {
                        SendMailFromSite(user.name, user.email, subject, body, contentType);
                    }
                }
            }
            catch (Exception ex)
            {
                this.RegisterEventForItem(new ItemKey(ItemTypeFactory.GetItemType<MessageSubscription>().IdItemType, idSubscription), Journaling.EventType.Error, 0, "Ошибка рассылки электронной почты", $"Неожиданная ошибка при выполнении рассылки №{idSubscription}.", ex);
            }
        }

        private string GetNoReplyAddress()
        {
            var address = AppCore.WebConfig.ReturnEmail;
            if (!string.IsNullOrEmpty(address)) return address;

            address = "no-reply@localhost";
            if (AppCore.ServerUrl != null) address = "no-reply@" + AppCore.ServerUrl.Host;

            return address;
        }

        void ICriticalMessagesReceiver.SendToAdmin(string subject, string body)
        {
            if (!string.IsNullOrEmpty(AppCore.WebConfig.CriticalMessagesEmail))
            {
                SendMail(
                    "Почтовый робот сайта",
                    GetNoReplyAddress(),
                    "admin",
                    AppCore.WebConfig.CriticalMessagesEmail,
                    null, null,
                    subject,
                    body,
                    ContentType.Text
                );
            }
        }

        #endregion
    }
}
