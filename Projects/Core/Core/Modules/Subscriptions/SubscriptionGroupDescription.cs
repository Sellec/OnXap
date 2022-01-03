using System;

namespace OnXap.Modules.Subscriptions
{
    /// <summary>
    /// Описание группы подписок.
    /// </summary>
    public class SubscriptionGroupDescription
    {
        internal SubscriptionGroupDescription()
        {
        }

        /// <summary>
        /// Идентификатор группы подписок.
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// Название группы подписок.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Уникальный ключ, позволяющий идентифицировать группу подписок.
        /// </summary>
        public Guid UniqueKey { get; internal set; }

        /// <summary>
        /// Равен true, если группа подписок была подтверждена регистрацией. Равен false, если описание группы подписок получено из кеша без подтверждения регистрацией.
        /// </summary>
        public bool IsConfirmed { get; internal set; }
    }
}