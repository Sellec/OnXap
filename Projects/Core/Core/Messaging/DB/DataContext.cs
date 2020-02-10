using OnUtils.Data;

namespace OnXap.Messaging.DB
{
    class DataContext : Core.Db.CoreContextBase
    {
        public IRepository<MessageQueue> MessageQueue { get; set; }
        public IRepository<MessageQueueHistory> MessageQueueHistory { get; set; }
    }
}
