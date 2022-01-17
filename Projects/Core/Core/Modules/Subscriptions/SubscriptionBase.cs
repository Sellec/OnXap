using System;
using System.Collections.Concurrent;

namespace OnXap.Modules.Subscriptions
{
    using Messaging;

    /// <summary>
    /// Предназначен для внутреннего использования.
    /// </summary>
    /// <typeparam name="TSelf"></typeparam>
    public abstract class SubscriptionBase<TSelf>
        where TSelf : SubscriptionBase<TSelf>
    {
        internal SubscriptionBase()
        { }

        /// <summary>
        /// </summary>
        internal protected abstract void OnRegisterBase();

        /// <summary>
        /// Описание подписки после регистрации.
        /// </summary>
        public SubscriptionDescription SubscriptionDescription { get; internal set; }
    }

    /// <summary>
    /// Базовый класс для подписок. 
    /// </summary>
    /// <typeparam name="TSubscription">Должен быть равен текущему типу для корректной работы привязок.</typeparam>
    /// <typeparam name="TParameters">Тип, определяющий параметры выполнения для подписки.</typeparam>
    public abstract class SubscriptionBase<TSubscription, TParameters> : SubscriptionBase<TSubscription>
        where TSubscription : SubscriptionBase<TSubscription, TParameters>
    {
        internal ConcurrentDictionary<Type, Action<IMessagingService, SubscriptionDescription, TParameters, MessagingContacts>> _services = new ConcurrentDictionary<Type, Action<IMessagingService, SubscriptionDescription, TParameters, MessagingContacts>>();

        /// <summary>
        /// </summary>
        protected sealed internal override void OnRegisterBase()
        {
            OnRegister();
        }

        /// <summary>
        /// Позволяет задать команды для выполнения для конкретного сервиса обмена сообщениями <typeparamref name="TMessagingService"/>.
        /// </summary>
        /// <param name="callback">Метод, выполняемый для конкретной подписки при вызове <see cref="SubscriptionParametrizedCall{TParameters}.ExecuteSubscription{TSubscription}"/>.</param>
        /// <remarks>Если метод вызывается повторно для конкретного сервиса в текущем наборе, то предыдущая команда будет перезаписана.</remarks>
        public void WithMessagingService<TMessagingService>(Action<SendInfo<TMessagingService, TParameters>> callback) 
            where TMessagingService : class, IMessagingService
        {
            var callback2 = new Action<IMessagingService, SubscriptionDescription, TParameters, MessagingContacts>((a, b, c, d) => callback(new SendInfo<TMessagingService, TParameters>()
            {
                MessagingService = (TMessagingService)a,
                SubscriptionDescription = b,
                Parameters = c,
                MessagingContacts = d
            }));
            _services.AddOrUpdate(typeof(TMessagingService), callback2, (t, old) => callback2);
        }

        /// <summary>
        /// Позволяет определить, задана ли команда для выполнения для конкретного сервиса обмена сообщениями <typeparamref name="TMessagingService"/>.
        /// </summary>
        public bool HasMessagingService<TMessagingService>() where TMessagingService : class, IMessagingService
        {
            return _services.ContainsKey(typeof(TMessagingService));
        }

        /// <summary>
        /// Вызывается во время регистрации подписки в сервисе <see cref="SubscriptionsManager"/>. 
        /// Позволяет определить команды для отдельных сервисов обмена сообщениями.
        /// </summary>
        /// <seealso cref="WithMessagingService{TMessagingService}(Action{SendInfo{TMessagingService, TParameters}})"/>
        /// <seealso cref="SubscriptionsManager.RegisterSubscription{TSubscription}(string, SubscriptionGroupDescription, TSubscription)"/>
        protected abstract void OnRegister();
    }
}
