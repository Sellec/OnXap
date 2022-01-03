using System;

namespace OnXap.Modules.Subscriptions
{
    /// <summary>
    /// Запрос для регистрации группы подписок.
    /// </summary>
    public class SubscriptionGroupRequest
    {
        /// <summary>
        /// Название группы подписок.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Уникальный ключ, позволяющий идентифицировать группу подписок.
        /// </summary>
        public Guid UniqueKey { get; set; }
    }
}