using Microsoft.EntityFrameworkCore;

namespace OnXap.Messaging.DB
{
    using Core.Db;

    class DataContext : CoreContextBase
    {
        public DbSet<MessageQueue> MessageQueue { get; set; }
        public DbSet<MessageQueueHistory> MessageQueueHistory { get; set; }

        public DbSet<Role> Role { get; set; }
        public DbSet<RoleUser> RoleUser { get; set; }
        public DbSet<User> User { get; set; }
    }
}
