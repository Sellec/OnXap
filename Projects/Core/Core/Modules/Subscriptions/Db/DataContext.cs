using Microsoft.EntityFrameworkCore;

namespace OnXap.Modules.Subscriptions.Db
{
    class DataContext : Core.Db.CoreContextBase
    {
        public DbSet<Messaging.DB.MessagingContact> MessagingContact { get; set; }
        public DbSet<Messaging.DB.MessagingContactData> MessagingContactData { get; set; }
        public DbSet<Subscription> Subscription { get; set; }
        public DbSet<SubscriptionGroup> SubscriptionGroup { get; set; }
        public DbSet<SubscriptionMessagingContact> SubscriptionMessagingContact { get; set; }
        public DbSet<SubscriptionUser> SubscriptionUser { get; set; }
        public DbSet<Core.Db.User> User { get; set; }
    }
}
