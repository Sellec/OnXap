﻿using OnXap.Messaging.Messages;
using System.Net.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnXap.Modules.MessagingEmail
{
    using Core.Data;
    using Core.Items;
    using Messaging;
    using Messaging.DB;

    /// <summary>
    /// Представляет сервис отправки электронных писем (Email).
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class EmailService : MessagingServiceBase<EmailMessage>, IDebugSupported
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
        /// <exception cref="ArgumentNullException">Возникает, если параметр <paramref name="email_from"/> не содержит значение.</exception>
        public void SendMail(string name_from, string email_from, string name_to, string email_to, Encoding data_charset, Encoding send_charset, string subject, string body, ContentType contentType, List<int> files = null)
        {
            if (string.IsNullOrEmpty(email_from)) throw new ArgumentNullException(nameof(email_from));

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
            var outgoing = GetDefaultOutgoingAddress();
            SendMail(outgoing.DisplayName, outgoing.Address, nameTo, emailTo, null, null, subject, body, contentType, files);
        }

        public void SendMailToDeveloper(string subject, string body, ContentType contentType, List<int> files = null)
        {
            var outgoing = GetDefaultOutgoingAddress();
            SendMail(outgoing.DisplayName, outgoing.Address, AppCore.WebConfig.DeveloperEmail, AppCore.WebConfig.DeveloperEmail, null, null, subject, body, contentType, files);
        }

        /// <summary>
        /// Возвращает обратный адрес по-умолчанию для писем, для которых отправитель не задан явно.
        /// </summary>
        /// <returns></returns>
        public MailAddress GetDefaultOutgoingAddress()
        {
            var address = "no-reply@localhost";
            if (AppCore.ServerUrl != null) address = "no-reply@" + AppCore.ServerUrl.Host;

            var cfg = AppCore.Get<Module>().GetConfiguration<ModuleConfiguration>();
            if (!string.IsNullOrEmpty(cfg.OutgoingAddress)) address = cfg.OutgoingAddress;

            return new MailAddress(address, cfg.OutgoingName);
        }

        #endregion
    }
}
