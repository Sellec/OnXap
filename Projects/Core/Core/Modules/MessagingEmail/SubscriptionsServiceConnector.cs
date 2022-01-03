using System.Linq;

namespace OnXap.Modules.MessagingEmail
{
    using Core.Data;
    using Subscriptions;

    /// <summary>
    /// Коннектор для упрощенной отправки любых уведомлений через EMail.
    /// </summary>
    public class SubscriptionsServiceConnector : MessagingServiceConnector<EmailService>
    {
        /// <summary>
        /// </summary>
        protected override void OnSendUniversal(SendInfoUniversal<EmailService> sendInfoUniversal)
        {
            using (var db = new Core.Db.CoreContext())
            {
                var query = db.Users.
                    In(sendInfoUniversal.UserIdList, x => x.IdUser).
                    Select(x => new { x.name, x.email });
                var data = query.ToList();

                data.
                    Where(x => !string.IsNullOrEmpty(x.email)).
                    ForEach(x =>
                    {
                        sendInfoUniversal.MessagingService.SendMailFromSite(
                            x.name,
                            x.email,
                            sendInfoUniversal.SubscriptionDescription.Name,
                            sendInfoUniversal.Message,
                            ContentType.Html
                        );
                    });
            }
        }
    }
}
