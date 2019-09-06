using OnUtils.Data;

namespace OnXap.Messaging.DB
{
    class DataContext : Core.DB.CoreContextBase
    {
        public IRepository<MessageQueue> MessageQueue { get; set; }
        public IRepository<MessageQueueHistory> MessageQueueHistory { get; set; }
    }
}
