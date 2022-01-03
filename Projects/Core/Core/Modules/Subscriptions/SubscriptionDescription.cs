using System;

namespace OnXap.Modules.Subscriptions
{
    /// <summary>
    /// Описание подписки.
    /// </summary>
    public class SubscriptionDescription
    {
        internal SubscriptionDescription()
        {
        }

        /// <summary>
        /// Идентификатор подписки.
        /// </summary>
        public int Id { get; internal set; }
        
        /// <summary>
        /// Название подписки.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Уникальный ключ, позволяющий идентифицировать подписку.
        /// </summary>
        public Guid UniqueKey { get; internal set; }

        /// <summary>
        /// Описание группы подписок, к которой относится подписка.
        /// </summary>
        public SubscriptionGroupDescription SubscriptionGroup { get; internal set; }

        /// <summary>
        /// Равен true, если подписка была подтверждена регистрацией. Равен false, если описание подписки получено из кеша без подтверждения регистрацией.
        /// </summary>
        public bool IsConfirmed { get; internal set; }
    }
}