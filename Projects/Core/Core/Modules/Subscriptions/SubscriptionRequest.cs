using System;

namespace OnXap.Modules.Subscriptions
{
    /// <summary>
    /// Запрос для регистрации подписки.
    /// </summary>
    public class SubscriptionRequest
    {
        /// <summary>
        /// Название подписки.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Уникальный ключ, позволяющий идентифицировать подписку.
        /// </summary>
        public Guid UniqueKey { get; set; }

        /// <summary>
        /// Описание группы подписок, к которой относится подписка.
        /// </summary>
        public SubscriptionGroupDescription SubscriptionGroup { get; set; }
    }
}