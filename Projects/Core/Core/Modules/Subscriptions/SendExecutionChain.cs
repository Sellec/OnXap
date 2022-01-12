using OnUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OnXap.Modules.Subscriptions
{
    using Messaging;

    /// <summary>
    /// Представляет набор указаний для отправки сообщений через ту или иную систему обмена сообщениями.
    /// </summary>
    public class SendExecutionChain
    {
        private SubscriptionsManager _manager;
        internal ConcurrentDictionary<Type, Action<IMessagingService, SubscriptionDescription, List<int>>> _services = new ConcurrentDictionary<Type, Action<IMessagingService, SubscriptionDescription, List<int>>>();
        private string _messageUniversal = null;
        private List<IMessagingService> _messageUniversalIgnoredServices = new List<IMessagingService>();
        internal readonly Guid _subscriptionUniqueKey;

        internal SendExecutionChain(SubscriptionsManager manager, Guid subscriptionUniqueKey)
        {
            _manager = manager;
        }

        /// <summary>
        /// Позволяет задать команды для выполнения для конкретного сервиса обмена сообщениями <typeparamref name="TMessagingService"/>.
        /// </summary>
        /// <param name="callback">Метод, выполняемый при вызове <see cref="Execute"/>.</param>
        /// <remarks>Если метод вызывается повторно для конкретного сервиса в текущем наборе, то предыдущая команда будет перезаписана.</remarks>
        public SendExecutionChain WithMessagingService<TMessagingService>(Action<SendInfo<TMessagingService>> callback) where TMessagingService : class, IMessagingService
        {
            var service = _manager.AppCore.Get<TMessagingService>();
            var callback2 = new Action<IMessagingService, SubscriptionDescription, List<int>>((a, b, c) => callback(new SendInfo<TMessagingService>()
            {
                MessagingService = (TMessagingService)a,
                SubscriptionDescription = b,
                UserIdList = c
            }));
            _services.AddOrUpdate(typeof(TMessagingService), callback2, (t, old) => callback2);
            return this;
        }

        /// <summary>
        /// Позволяет задать сообщение <paramref name="message"/>, которое должно быть отправлено через все сервисы обмена сообщениями, для которых заданы универсальные 
        /// коннекторы (см. <see cref="SubscriptionsManager.SetMessagingServiceSendAsUniversalConnector{T}(MessagingServiceSendAsUniversalConnector{T})"/>). Сервисы,
        /// для которых в данном наборе заданы персональные команды отправки (см. <see cref="WithMessagingService{TMessagingService}(Action{SendInfo{TMessagingService}})"/>) будут проигнорированы во время поиска коннекторов.
        /// </summary>
        /// <remarks>Если метод вызывается повторно в текущем наборе, то предыдущая команда будет перезаписана.</remarks>
        public SendExecutionChain WithUniversal(string message)
        {
            _messageUniversal = message;
            return this;
        }

        /// <summary>
        /// Запускает выполнение набора заданных команд.
        /// </summary>
        /// <returns></returns>
        public ExecutionResult<Exception> Execute()
        {
            if (!string.IsNullOrEmpty(_messageUniversal))
            {
                var callback2 = new Action<IMessagingService, SubscriptionDescription, List<int>>((a, b, c) => _manager.SendInternal(b, c, _messageUniversal, _messageUniversalIgnoredServices));
                _services.AddOrUpdate(typeof(SubscriptionsManager), callback2, (t, old) => callback2);
            }
            var result = _manager.SendExecuteInternal(this);
            Clear();
            return result;
        }

        /// <summary>
        /// Позволяет определить, задана ли команда для выполнения для конкретного сервиса обмена сообщениями <typeparamref name="TMessagingService"/>.
        /// </summary>
        public bool HasMessagingService<TMessagingService>() where TMessagingService : class, IMessagingService
        {
            return _services.ContainsKey(typeof(TMessagingService));
        }

        internal void Clear()
        {
            _manager = null;
            _services.Clear();
            _messageUniversalIgnoredServices.Clear();
        }
    }
}
