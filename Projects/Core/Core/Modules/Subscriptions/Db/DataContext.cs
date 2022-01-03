using Microsoft.EntityFrameworkCore;

namespace OnXap.Modules.Subscriptions.Db
{
    class DataContext : Core.Db.CoreContextBase
    {
        public DbSet<SubscriptionGroup> SubscriptionGroup { get; set; }
        public DbSet<Subscription> Subscription { get; set; }
        public DbSet<SubscriptionUser> SubscriptionUser { get; set; }
    }
}
