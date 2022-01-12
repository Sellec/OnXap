using System.Linq;

namespace OnXap.Modules.Subscriptions
{
    using Core.Data;
    using MessagingEmail;

    class MessagingServiceSendAsUniversalConnectorDefault : MessagingServiceSendAsUniversalConnector<EmailService>
    {
        protected override void OnSend(SendAsUniversalInfo<EmailService> sendInfoUniversal)
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
