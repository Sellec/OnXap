using OnUtils.Data;

namespace OnXap.Messaging.DB
{
    using Core.Db;

    class DataContext : CoreContextBase
    {
        public IRepository<MessageQueue> MessageQueue { get; set; }
        public IRepository<MessageQueueHistory> MessageQueueHistory { get; set; }

        public IRepository<MessageSubscription> MessageSubscription { get; set; }
        public IRepository<MessageSubscriptionRole> MessageSubscriptionRole { get; set; }

        public IRepository<Role> Role { get; set; }
        public IRepository<RoleUser> RoleUser { get; set; }
        public IRepository<User> User { get; set; }

    }
}
