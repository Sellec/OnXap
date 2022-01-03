using OnUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using FlexLabs.EntityFrameworkCore;

namespace OnXap.Modules.Subscriptions
{
    using Messaging;

    /// <summary>
    /// Позволяет управлять подписками.
    /// </summary>
    public class SubscriptionsManager : Core.CoreComponentBase, Core.IComponentSingleton
    {
        private ConcurrentDictionary<Guid, SubscriptionGroupDescription> _subscriptionGroups = new ConcurrentDictionary<Guid, SubscriptionGroupDescription>();
        private ConcurrentDictionary<Guid, SubscriptionDescription> _subscriptions = new ConcurrentDictionary<Guid, SubscriptionDescription>();
        private ConcurrentDictionary<Type, MessagingServiceConnectorInternal> _messagingServiceConnectors = new ConcurrentDictionary<Type, MessagingServiceConnectorInternal>();

        #region Запуск задач
        /// <summary>
        /// </summary>
        protected sealed override void OnStarting()
        {
            this.RegisterJournal("Журнал менеджера подписок");
            SubscriptionGroupSystem = RegisterSubscriptionGroup(new SubscriptionGroupRequest()
            {
                Name = "Система",
                UniqueKey = "System".GenerateGuid()
            });
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStarted()
        {
            //AppCore.Get<SubscriptionsManager>().ConnectMessagingService(new ec());
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {

        }
        #endregion

        #region Управление группами подписок.
        /// <summary>
        /// Позволяет зарегистрировать группу подписок. Если такая группа ранее была зарегистрирована, то обновляет параметры ранее зарегистрированной.
        /// </summary>
        /// <returns>Возвращает описание зарегистрированной группы подписок.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="subscriptionGroupRequest"/> равен null.</exception>
        /// <exception cref="ArgumentException">Возникает, если не указано имя группы подписок.</exception>
        /// <exception cref="ArgumentException">Возникает, если уникальный ключ группы подписок равен <see cref="Guid.Empty"/>.</exception>
        [ApiIrreversible]
        public SubscriptionGroupDescription RegisterSubscriptionGroup(SubscriptionGroupRequest subscriptionGroupRequest)
        {
            if (subscriptionGroupRequest == null) throw new ArgumentNullException(nameof(subscriptionGroupRequest));
            if (string.IsNullOrEmpty(subscriptionGroupRequest.Name)) throw new ArgumentException("Имя не может быть пустым.", nameof(subscriptionGroupRequest.Name));
            if (subscriptionGroupRequest.UniqueKey == Guid.Empty) throw new ArgumentException("Уникальный ключ не может быть пустым.", nameof(subscriptionGroupRequest.UniqueKey));

            try
            {
                var description = _subscriptionGroups.AddOrUpdate(subscriptionGroupRequest.UniqueKey,
                    key => UpdateSubscriptionGroup(new SubscriptionGroupDescription(), subscriptionGroupRequest),
                    (key, old) => UpdateSubscriptionGroup(old, subscriptionGroupRequest)
                );
                return description;
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.CriticalError, "Ошибка во время регистрации группы подписок", null, ex);
                throw new Exception("Неожиданная ошибка во время регистрации группы подписок.");
            }
        }

        private SubscriptionGroupDescription UpdateSubscriptionGroup(SubscriptionGroupDescription subscriptionGroupDescription, SubscriptionGroupRequest subscriptionGroupRequest)
        {
            using (var db = new Db.DataContext())
            using (var scope = db.CreateScope(System.Transactions.TransactionScopeOption.Suppress))
            {
                var subscriptionGroupDb = db.SubscriptionGroup.Where(x => x.UniqueKey == subscriptionGroupRequest.UniqueKey).FirstOrDefault();
                if (subscriptionGroupDb == null)
                {
                    subscriptionGroupDb = new Db.SubscriptionGroup()
                    {
                        NameGroup = subscriptionGroupRequest.Name,
                        UniqueKey = subscriptionGroupRequest.UniqueKey
                    };
                    db.SubscriptionGroup.Add(subscriptionGroupDb);
                    db.SaveChanges();
                }

                subscriptionGroupDb.NameGroup = subscriptionGroupRequest.Name;
                db.SaveChanges();

                subscriptionGroupDescription.Id = subscriptionGroupDb.IdGroup;
                subscriptionGroupDescription.Name = subscriptionGroupRequest.Name;
                subscriptionGroupDescription.IsConfirmed = true;
                subscriptionGroupDescription.UniqueKey = subscriptionGroupRequest.UniqueKey;
            }
            return subscriptionGroupDescription;
        }

        /// <summary>
        /// Возвращает список групп подписок.
        /// </summary>
        /// <param name="onlyConfirmed">Если равно true, то возвращает только подтвержденные группы (см. <see cref="SubscriptionGroupDescription.IsConfirmed"/>.</param>
        [ApiIrreversible]
        public List<SubscriptionGroupDescription> GetSubscriptionGroups(bool onlyConfirmed)
        {
            try
            {
                var list = _subscriptionGroups.Values.ToDictionary(x => x.Id, x => x);
                if (!onlyConfirmed)
                {
                    using (var db = new Db.DataContext())
                    using (var scope = db.CreateScope(System.Transactions.TransactionScopeOption.Suppress))
                    {
                        var query = db.SubscriptionGroup;
                        var dbList = query.ToList();
                        foreach (var rowDb in dbList)
                        {
                            if (list.ContainsKey(rowDb.IdGroup)) continue;

                            var description = CreateUnconfirmedSubscriptionGroupDescription(rowDb);
                            list[rowDb.IdGroup] = description;
                        }
                    }
                }
                return list.Values.ToList();
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.CriticalError, "Ошибка во время получения списка групп подписок", null, ex);
                throw new Exception("Неожиданная ошибка во время получения списка групп подписок.");
            }
        }

        private SubscriptionGroupDescription CreateUnconfirmedSubscriptionGroupDescription(Db.SubscriptionGroup rowDb)
        {
            return new SubscriptionGroupDescription
            {
                Id = rowDb.IdGroup,
                Name = rowDb.NameGroup,
                IsConfirmed = false,
                UniqueKey = rowDb.UniqueKey,
            };
        }
        #endregion

        #region Управление подписками.
        /// <summary>
        /// Позволяет зарегистрировать подписку. Если такая подписка ранее была зарегистрирована, то обновляет параметры ранее зарегистрированной.
        /// </summary>
        /// <returns>Возвращает описание зарегистрированной подписки.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="subscriptionRequest"/> равен null.</exception>
        /// <exception cref="ArgumentException">Возникает, если значение свойства <see cref="SubscriptionRequest.Name"/> задано пустым.</exception>
        /// <exception cref="ArgumentException">Возникает, если значение свойства <see cref="SubscriptionRequest.SubscriptionGroup"/> задано пустым.</exception>
        /// <exception cref="ArgumentException">Возникает, если группа подписок, указанная в свойстве <see cref="SubscriptionRequest.SubscriptionGroup"/>, не найдена как подтвержденная.</exception>
        /// <exception cref="ArgumentException">Возникает, если <see cref="SubscriptionRequest.UniqueKey"/> равен <see cref="Guid.Empty"/>.</exception>
        [ApiIrreversible]
        public SubscriptionDescription RegisterSubscription(SubscriptionRequest subscriptionRequest)
        {
            if (subscriptionRequest == null) throw new ArgumentNullException(nameof(subscriptionRequest));
            if (string.IsNullOrEmpty(subscriptionRequest.Name)) throw new ArgumentException("Имя не может быть пустым.", nameof(subscriptionRequest.Name));
            if (subscriptionRequest.SubscriptionGroup == null) throw new ArgumentException("Не указана группа подписок.", nameof(subscriptionRequest.SubscriptionGroup));
            if (!_subscriptionGroups.TryGetValue(subscriptionRequest.SubscriptionGroup.UniqueKey, out var subscriptionGroupDescription)) throw new ArgumentException("УКазанная группа подписок не найдена.", nameof(subscriptionRequest.SubscriptionGroup));
            if (subscriptionRequest.UniqueKey == Guid.Empty) throw new ArgumentException("Уникальный ключ не может быть пустым.", nameof(subscriptionRequest.UniqueKey));

            try
            {
                var description = _subscriptions.AddOrUpdate(subscriptionRequest.UniqueKey,
                    key => UpdateSubscription(new SubscriptionDescription(), subscriptionRequest, subscriptionGroupDescription),
                    (key, old) => UpdateSubscription(old, subscriptionRequest, subscriptionGroupDescription)
                );
                return description;
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.CriticalError, "Ошибка во время регистрации подписки", null, ex);
                throw new Exception("Неожиданная ошибка во время регистрации подписки.");
            }
        }

        private SubscriptionDescription UpdateSubscription(SubscriptionDescription subscriptionDescription, SubscriptionRequest subscriptionRequest, SubscriptionGroupDescription subscriptionGroupDescription)
        {
            using (var db = new Db.DataContext())
            using (var scope = db.CreateScope(System.Transactions.TransactionScopeOption.Suppress))
            {
                var rowDb = db.Subscription.Where(x => x.UniqueKey == subscriptionRequest.UniqueKey).FirstOrDefault();
                if (rowDb == null)
                {
                    rowDb = new Db.Subscription()
                    {
                        NameSubscription = subscriptionRequest.Name,
                        UniqueKey = subscriptionRequest.UniqueKey,
                        IdGroup = subscriptionGroupDescription.Id
                    };
                    db.Subscription.Add(rowDb);
                    db.SaveChanges();
                }

                rowDb.NameSubscription = subscriptionRequest.Name;
                rowDb.IdGroup = subscriptionGroupDescription.Id;
                db.SaveChanges();

                subscriptionDescription.Id = rowDb.IdGroup;
                subscriptionDescription.Name = subscriptionRequest.Name;
                subscriptionDescription.IsConfirmed = true;
                subscriptionDescription.UniqueKey = subscriptionRequest.UniqueKey;
                subscriptionDescription.SubscriptionGroup = subscriptionGroupDescription;
            }
            return subscriptionDescription;
        }

        /// <summary>
        /// Возвращает список подписок.
        /// </summary>
        /// <param name="onlyConfirmed">Если равно true, то возвращает только подтвержденные подписки (см. <see cref="SubscriptionDescription.IsConfirmed"/>.</param>
        [ApiIrreversible]
        public List<SubscriptionDescription> GetSubscriptions(bool onlyConfirmed)
        {
            try
            {
                var list = _subscriptions.Values.ToDictionary(x => x.Id, x => x);
                if (!onlyConfirmed)
                {
                    using (var db = new Db.DataContext())
                    using (var scope = db.CreateScope(System.Transactions.TransactionScopeOption.Suppress))
                    {
                        var query = from s in db.Subscription
                                    join sg in db.SubscriptionGroup on s.IdGroup equals sg.IdGroup
                                    select new { s, sg };
                        var dbList = query.ToList();
                        foreach (var rowDb in dbList)
                        {
                            if (list.ContainsKey(rowDb.s.IdSubscription)) continue;

                            var subscriptionGroupDescription = _subscriptionGroups.Values.FirstOrDefault(x => x.Id == rowDb.s.IdGroup);
                            if (subscriptionGroupDescription == null) subscriptionGroupDescription = CreateUnconfirmedSubscriptionGroupDescription(rowDb.sg);

                            var description = new SubscriptionDescription
                            {
                                Id = rowDb.s.IdSubscription,
                                Name = rowDb.s.NameSubscription,
                                IsConfirmed = false,
                                UniqueKey = rowDb.s.UniqueKey,
                                SubscriptionGroup = subscriptionGroupDescription
                            };
                            list[rowDb.s.IdSubscription] = description;
                        }
                    }
                }
                return list.Values.ToList();
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.CriticalError, "Ошибка во время получения списка подписок", null, ex);
                throw new Exception("Неожиданная ошибка во время получения списка подписок.");
            }
        }

        [ApiIrreversible]
        public void UpdateSubscribers(SubscriptionDescription subscriptionDescription, ChangeType changeType, int[] userIdList)
        {
            if (subscriptionDescription == null) throw new ArgumentNullException(nameof(subscriptionDescription));
            UpdateSubscribersInternal(subscriptionDescription.UniqueKey, changeType, userIdList);
        }

        [ApiIrreversible]
        public void UpdateSubscribers(Guid subscriptionUniqueKey, ChangeType changeType, int[] userIdList)
        {
            var result = UpdateSubscribersInternal(subscriptionUniqueKey, changeType, userIdList);
            if (!result.IsSuccess) throw result.Result;
        }

        [ApiIrreversible]
        public ExecutionResult TryUpdateSubscribers(Guid subscriptionUniqueKey, ChangeType changeType, int[] userIdList)
        {
            var result = UpdateSubscribersInternal(subscriptionUniqueKey, changeType, userIdList);
            return new ExecutionResult(result.IsSuccess, result.Message);
        }

        private ExecutionResult<Exception> UpdateSubscribersInternal(Guid subscriptionUniqueKey, ChangeType changeType, int[] userIdList)
        {
            if (!_subscriptions.TryGetValue(subscriptionUniqueKey, out var subscriptionDescription2))
                return new ExecutionResult<Exception>(
                    false,
                    "Указанная подписка не найдена среди подтвержденных",
                    new ArgumentException("Указанная подписка не найдена среди подтвержденных", nameof(subscriptionUniqueKey)));

            try
            {
                using (var db = new Db.DataContext())
                using (var scope = db.CreateScope())
                {
                    if (changeType == ChangeType.Append || changeType == ChangeType.Replace)
                    {
                        var list = userIdList.Select(x => new Db.SubscriptionUser() { IdSubscription = subscriptionDescription2.Id, IdUser = x }).ToList();
                        db.SubscriptionUser.UpsertRange(list).On(x => new { x.IdUser, x.IdSubscription }).NoUpdate().Run();
                    }

                    if (changeType == ChangeType.Replace)
                    {
                        var list = db.SubscriptionUser.Where(x => x.IdSubscription == subscriptionDescription2.Id).ToList().Where(x => !userIdList.Contains(x.IdUser)).ToList();
                        db.SubscriptionUser.RemoveRange(list);
                        db.SaveChanges();
                    }

                    if (changeType == ChangeType.Remove)
                    {
                        var list = db.SubscriptionUser.Where(x => x.IdSubscription == subscriptionDescription2.Id).ToList().Where(x => userIdList.Contains(x.IdUser)).ToList();
                        db.SubscriptionUser.RemoveRange(list);
                        db.SaveChanges();
                    }

                    scope.Complete();
                }
                return new ExecutionResult<Exception>(true);
            }
            catch (Exception ex)
            {
                var msg = $"Рассылка: '{subscriptionDescription2.Id}' / '{subscriptionUniqueKey}' / '{subscriptionDescription2.Name}'.\r\n";
                this.RegisterEvent(Journaling.EventType.Error, "Неожиданная ошибка обновления списка подписчиков", msg, ex);
                return new ExecutionResult<Exception>(false, "Неожиданная ошибка обновления списка подписчиков", ex);
            }
        }
        #endregion

        #region Подключение сервисов отправки/получения сообщений.
        [ApiIrreversible]
        public void ConnectMessagingService<T>(MessagingServiceConnector<T> messagingServiceConnector) where T : IMessageService
        {
            _messagingServiceConnectors.AddOrUpdate(typeof(T), messagingServiceConnector, (t, old) => messagingServiceConnector);
        }

        [ApiIrreversible]
        public void SendFromSubscriptionUniversal(SubscriptionDescription subscriptionDescription, string message)
        {
            if (subscriptionDescription == null) throw new ArgumentNullException(nameof(subscriptionDescription));
            SendFromSubscriptionUniversal(subscriptionDescription.UniqueKey, message);
        }

        [ApiIrreversible]
        public void SendFromSubscriptionUniversal(Guid subscriptionUniqueKey, string message)
        {
            var result = SendFromSubscriptionUniversalInternal(subscriptionUniqueKey, message);
            if (!result.IsSuccess) throw result.Result;
        }

        [ApiIrreversible]
        public ExecutionResult TrySendFromSubscriptionUniversal(Guid subscriptionUniqueKey, string message)
        {
            var result = SendFromSubscriptionUniversalInternal(subscriptionUniqueKey, message);
            return new ExecutionResult(result.IsSuccess, result.Message);
        }

        private ExecutionResult<Exception> SendFromSubscriptionUniversalInternal(Guid subscriptionUniqueKey, string message)
        {
            if (!_subscriptions.TryGetValue(subscriptionUniqueKey, out var subscriptionDescription2))
                return new ExecutionResult<Exception>(
                    false, 
                    "Указанная подписка не найдена среди подтвержденных", 
                    new ArgumentException("Указанная подписка не найдена среди подтвержденных", nameof(subscriptionUniqueKey)));

            var connectorsSnapshot = _messagingServiceConnectors.ToList();
            if (connectorsSnapshot.IsNullOrEmpty()) return new ExecutionResult<Exception>(true);

            try
            {
                var userIdList = new List<int>();
                using (var db = new Db.DataContext())
                {
                    var query = db.SubscriptionUser.Where(x => x.IdSubscription == subscriptionDescription2.Id).Select(x => new { x.IdUser });
                    var data = query.ToList();
                    userIdList = data.Select(x => x.IdUser).ToList();
                }

                foreach (var connector in connectorsSnapshot)
                {
                    try
                    {
                        var messagingService = AppCore.Get<IMessageService>(connector.Key);
                        if (messagingService == null)
                        {
                            var msg = $"Рассылка: '{subscriptionDescription2.Id}' / '{subscriptionUniqueKey}' / '{subscriptionDescription2.Name}'.\r\n";
                            msg += $"Коннектор: '{connector.Key.FullName}' / '{connector.Value.GetType().FullName}'.";
                            this.RegisterEvent(Journaling.EventType.Error, "Ошибка поиска сервиса отправки/получения сообщений для рассылки", msg);
                        }
                        else
                        {
                            connector.Value.SendUniversal(new SendInfoUniversal<IMessageService>()
                            {
                                Message = message,
                                MessagingService = messagingService,
                                SubscriptionDescription = subscriptionDescription2,
                                UserIdList = userIdList
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = $"Рассылка: '{subscriptionDescription2.Id}' / '{subscriptionUniqueKey}' / '{subscriptionDescription2.Name}'.\r\n";
                        msg += $"Коннектор: '{connector.Key.FullName}' / '{connector.Value.GetType().FullName}'.";
                        this.RegisterEvent(Journaling.EventType.Error, "Неожиданная ошибка рассылки", msg, ex);
                    }
                }
                return new ExecutionResult<Exception>(true);
            }
            catch (Exception ex)
            {
                var msg = $"Рассылка: '{subscriptionDescription2.Id}' / '{subscriptionUniqueKey}' / '{subscriptionDescription2.Name}'.\r\n";
                this.RegisterEvent(Journaling.EventType.Error, "Неожиданная ошибка рассылки", msg, ex);
                return new ExecutionResult<Exception>(false, "Неожиданная ошибка рассылки", ex);
            }
        }
        #endregion

        #region Свойства
        /// <summary>
        /// Описание группы подписок "Система".
        /// </summary>
        public SubscriptionGroupDescription SubscriptionGroupSystem { get; private set; }
        #endregion
    }
}
