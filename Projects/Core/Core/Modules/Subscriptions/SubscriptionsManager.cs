using Microsoft.EntityFrameworkCore;
using OnUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OnXap.Modules.Subscriptions
{
    using Messaging;

    /// <summary>
    /// Позволяет управлять подписками.
    /// </summary>
    public class SubscriptionsManager : Core.CoreComponentBase, Core.IComponentSingleton
    {
        private ConcurrentDictionary<Guid, SubscriptionGroupDescription> _subscriptionGroups = new ConcurrentDictionary<Guid, SubscriptionGroupDescription>();
        private ConcurrentDictionary<Type, Tuple<object, SubscriptionDescription>> _subscriptions = new ConcurrentDictionary<Type, Tuple<object, SubscriptionDescription>>();

        #region Запуск задач
        /// <summary>
        /// </summary>
        protected sealed override void OnStarting()
        {
            this.RegisterJournal("Журнал менеджера подписок");
            SubscriptionGroupSystem = RegisterSubscriptionGroup("Система", "System".GenerateGuid());
        }

        #endregion

        #region Управление группами подписок.
        /// <summary>
        /// Позволяет зарегистрировать группу подписок. Если такая группа ранее была зарегистрирована, то обновляет параметры ранее зарегистрированной.
        /// </summary>
        /// <returns>Возвращает описание зарегистрированной группы подписок.</returns>
        /// <exception cref="ArgumentException">Возникает, если параметр <paramref name="name"/> задан пустым.</exception>
        /// <exception cref="ArgumentException">Возникает, если параметр <paramref name="uniqueKey"/> равен <see cref="Guid.Empty"/>.</exception>
        [ApiIrreversible]
        public SubscriptionGroupDescription RegisterSubscriptionGroup(string name, Guid uniqueKey)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Имя не может быть пустым.", nameof(name));
            if (uniqueKey == Guid.Empty) throw new ArgumentException("Уникальный ключ не может быть пустым.", nameof(uniqueKey));

            try
            {
                var description = _subscriptionGroups.AddOrUpdate(uniqueKey,
                    key => UpdateSubscriptionGroup(new SubscriptionGroupDescription(), name, uniqueKey),
                    (key, old) => UpdateSubscriptionGroup(old, name, uniqueKey)
                );
                return description;
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.CriticalError, "Ошибка во время регистрации группы подписок", null, ex);
                throw new Exception("Неожиданная ошибка во время регистрации группы подписок.");
            }
        }

        private SubscriptionGroupDescription UpdateSubscriptionGroup(SubscriptionGroupDescription subscriptionGroupDescription, string name, Guid uniqueKey)
        {
            using (var db = new Db.DataContext())
            using (var scope = db.CreateScope(System.Transactions.TransactionScopeOption.Suppress))
            {
                var subscriptionGroupDb = db.SubscriptionGroup.Where(x => x.UniqueKey == uniqueKey).FirstOrDefault();
                if (subscriptionGroupDb == null)
                {
                    subscriptionGroupDb = new Db.SubscriptionGroup()
                    {
                        NameGroup = name,
                        UniqueKey = uniqueKey
                    };
                    db.SubscriptionGroup.Add(subscriptionGroupDb);
                    db.SaveChanges();
                }

                subscriptionGroupDb.NameGroup = name;
                db.SaveChanges();

                subscriptionGroupDescription.Id = subscriptionGroupDb.IdGroup;
                subscriptionGroupDescription.Name = name;
                subscriptionGroupDescription.IsConfirmed = true;
                subscriptionGroupDescription.UniqueKey = uniqueKey;
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
        /// Позволяет зарегистрировать подписку на базе типа <typeparamref name="TSubscription"/>. Если такая подписка ранее была зарегистрирована, то обновляет параметры ранее зарегистрированной.
        /// </summary>
        /// <returns>Возвращает описание зарегистрированной подписки.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="instance"/> равен null.</exception>
        /// <exception cref="ArgumentException">Возникает, если значение параметра <paramref name="name"/> задано пустым.</exception>
        /// <exception cref="ArgumentException">Возникает, если значение параметра <paramref name="group"/> равно null.</exception>
        /// <exception cref="ArgumentException">Возникает, если группа подписок <paramref name="group"/> не найдена как подтвержденная.</exception>
        [ApiIrreversible]
        public SubscriptionDescription RegisterSubscription<TSubscription>(string name, SubscriptionGroupDescription group, TSubscription instance)
            where TSubscription : SubscriptionBase<TSubscription>
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Имя не может быть пустым.", nameof(name));
            if (group == null) throw new ArgumentException("Не указана группа подписок.", nameof(group));
            if (!_subscriptionGroups.TryGetValue(group.UniqueKey, out var subscriptionGroupDescription)) throw new ArgumentException("Указанная группа подписок не найдена.", nameof(group));
            var typeInfo = OnUtils.Types.TypeHelpers.ExtractGenericType(typeof(TSubscription), typeof(SubscriptionBase<,>));
            if (typeInfo == null) throw new ArgumentException($"Тип '{instance.GetType()}' должен реализовывать '{typeof(SubscriptionBase<,>)}'.", nameof(instance));

            try
            {
                instance.OnRegisterBase();
                var uniqueKey = typeof(TSubscription).FullName.GenerateGuid();
                var result = _subscriptions.AddOrUpdate(typeof(TSubscription),
                    key => new Tuple<object, SubscriptionDescription>(instance, UpdateSubscription(name, uniqueKey, subscriptionGroupDescription, new SubscriptionDescription())),
                    (key, old) => new Tuple<object, SubscriptionDescription>(instance, UpdateSubscription(name, uniqueKey, subscriptionGroupDescription, old.Item2))
                );
                instance.SubscriptionDescription = result.Item2;

                var types = typeInfo.GetGenericArguments();
                typeof(SubscriptionsManager).
                    GetMethod(nameof(OnRegister), BindingFlags.Instance | BindingFlags.NonPublic).
                    MakeGenericMethod(types[0], types[1]).Invoke(this, new object[] { instance });

                return result.Item2;
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.CriticalError, "Ошибка во время регистрации подписки", null, ex);
                throw new Exception("Неожиданная ошибка во время регистрации подписки.");
            }
        }

        private SubscriptionDescription UpdateSubscription(
            string name,
            Guid uniqueKey,
            SubscriptionGroupDescription subscriptionGroupDescription,
            SubscriptionDescription subscriptionDescription)
        {
            using (var db = new Db.DataContext())
            using (var scope = db.CreateScope(System.Transactions.TransactionScopeOption.Suppress))
            {
                var rowDb = db.Subscription.Where(x => x.UniqueKey == uniqueKey).FirstOrDefault();
                if (rowDb == null)
                {
                    rowDb = new Db.Subscription()
                    {
                        NameSubscription = name,
                        UniqueKey = uniqueKey,
                        IdGroup = subscriptionGroupDescription.Id
                    };
                    db.Subscription.Add(rowDb);
                    db.SaveChanges();
                }

                rowDb.NameSubscription = name;
                rowDb.IdGroup = subscriptionGroupDescription.Id;
                db.SaveChanges();

                subscriptionDescription.Id = rowDb.IdSubscription;
                subscriptionDescription.Name = name;
                subscriptionDescription.IsConfirmed = true;
                subscriptionDescription.SubscriptionGroup = subscriptionGroupDescription;
            }
            return subscriptionDescription;
        }

        /// <summary>
        /// Вызывается в конце регистрациии подписки <typeparamref name="TSubscription"/>.
        /// </summary>
        /// <param name="instance">Экземпляр объекта, олицетворяющего подписку.</param>
        /// <seealso cref="RegisterSubscription{TSubscription}(string, SubscriptionGroupDescription, TSubscription)"/>
        /// <seealso cref="SubscriptionParametrizedCall{TParameters}.ExecuteSubscription{TSubscription}"/>
        /// <seealso cref="AsParametrized{TParameters}(TParameters)"/>
        protected virtual void OnRegister<TSubscription, TParameters>(TSubscription instance)
            where TSubscription : SubscriptionBase<TSubscription, TParameters>
        {
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
                var list = _subscriptions.Values.ToDictionary(x => x.Item2.Id, x => x.Item2);
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
        public ExecutionResult UpdateSubscribers(SubscriptionDescription subscriptionDescription, ChangeType changeType, int[] userIdList)
        {
            var result = UpdateSubscribersInternal(subscriptionDescription, changeType, userIdList);
            return new ExecutionResult(result.IsSuccess, result.Message);
        }

        private ExecutionResult<Exception> UpdateSubscribersInternal(SubscriptionDescription subscriptionDescription, ChangeType changeType, int[] userIdList)
        {
            var subscriptionInfo = _subscriptions.FirstOrDefault(x => x.Value.Item2.Id == subscriptionDescription.Id).Value.Item2;
            if (subscriptionInfo != null)
                return new ExecutionResult<Exception>(
                    false,
                    "Указанная подписка не найдена среди подтвержденных",
                    new ArgumentException("Указанная подписка не найдена среди подтвержденных", nameof(subscriptionDescription)));

            try
            {
                using (var db = new Db.DataContext())
                using (var scope = db.CreateScope())
                {
                    if (changeType == ChangeType.Append || changeType == ChangeType.Replace)
                    {
                        var list = userIdList.Select(x => new Db.SubscriptionUser() { IdSubscription = subscriptionInfo.Id, IdUser = x }).ToList();
                        db.SubscriptionUser.UpsertRange(list).On(x => new { x.IdUser, x.IdSubscription }).NoUpdate().Run();
                    }

                    if (changeType == ChangeType.Replace)
                    {
                        var list = db.SubscriptionUser.Where(x => x.IdSubscription == subscriptionInfo.Id).ToList().Where(x => !userIdList.Contains(x.IdUser)).ToList();
                        db.SubscriptionUser.RemoveRange(list);
                        db.SaveChanges();
                    }

                    if (changeType == ChangeType.Remove)
                    {
                        var list = db.SubscriptionUser.Where(x => x.IdSubscription == subscriptionInfo.Id).ToList().Where(x => userIdList.Contains(x.IdUser)).ToList();
                        db.SubscriptionUser.RemoveRange(list);
                        db.SaveChanges();
                    }

                    scope.Complete();
                }
                return new ExecutionResult<Exception>(true);
            }
            catch (Exception ex)
            {
                var msg = $"Рассылка: '{subscriptionInfo.Id}' / '{subscriptionInfo.Name}'.\r\n";
                this.RegisterEvent(Journaling.EventType.Error, "Неожиданная ошибка обновления списка подписчиков", msg, ex);
                return new ExecutionResult<Exception>(false, "Неожиданная ошибка обновления списка подписчиков", ex);
            }
        }
        #endregion

        #region Подключение сервисов отправки/получения сообщений.
        /// <summary>
        /// Позволяет указать параметры выполнения для подписки. Возвращает объект <see cref="SubscriptionParametrizedCall{TParameters}"/>, 
        /// для которого необходимо выполнить вызов <see cref="SubscriptionParametrizedCall{TParameters}.ExecuteSubscription{TSubscription}"/>.
        /// </summary>
        [ApiIrreversible]
        public SubscriptionParametrizedCall<TParameters> AsParametrized<TParameters>(TParameters parameters)
        {
            return new SubscriptionParametrizedCall<TParameters>()
            {
                _manager = this,
                _parameters = parameters
            };
        }

        [ApiIrreversible]
        internal ExecutionResult<Exception> ExecuteSubscription<TSubscription, TParameters>(TParameters parameters)
            where TSubscription : SubscriptionBase<TSubscription, TParameters>
        {

            if (!_subscriptions.TryGetValue(typeof(TSubscription), out var info))
                return new ExecutionResult<Exception>(
                    false,
                    "Указанная подписка не найдена среди подтвержденных",
                    new ArgumentException("Указанная подписка не найдена среди подтвержденных", nameof(TSubscription)));

            var subscriptionInstance = (TSubscription)info.Item1;

            try
            {
                var messagingContacts = new MessagingContacts();
                using (var db = new Db.DataContext())
                {
                    var queryFromUsers = db.SubscriptionUser.Where(x => x.IdSubscription == subscriptionInstance.SubscriptionDescription.Id).Select(x => new { x.IdUser });
                    var dataFromUsers = queryFromUsers.ToList();
                    var userIdList = dataFromUsers.Select(x => x.IdUser).ToList();

                    var queryFromContacts = from smc in db.SubscriptionMessagingContact
                                            join mc in db.MessagingContact on smc.IdMessagingContact equals mc.IdMessagingContact
                                            join mcd in db.MessagingContactData on mc.IdMessagingContact equals mcd.IdMessagingContactData
                                            where smc.IdSubscription == subscriptionInstance.SubscriptionDescription.Id
                                            select new
                                            {
                                                mc.NameFull,
                                                mcd.IdMessagingContact,
                                                mcd.IdMessagingServiceType,
                                                mcd.Data
                                            };

                    var types = new Dictionary<int, Type>();
                    var getType = new Func<int, Type>(idMessagingServiceType =>
                    {
                        if (types.TryGetValue(idMessagingServiceType, out var t)) return t;
                        t = Core.Items.ItemTypeFactory.GetClsType(idMessagingServiceType);
                        types[idMessagingServiceType] = t;
                        return t;
                    });

                    var dataFromContacts = queryFromContacts.
                        ToList().
                        GroupBy(x => new { x.IdMessagingContact, x.NameFull }, x => new { x.Data, x.IdMessagingServiceType }).
                        Select(x => new MessagingContact()
                        {
                            Id = x.Key.IdMessagingContact,
                            NameFull = x.Key.NameFull,
                            _data = x.GroupBy(y => y.IdMessagingServiceType, y => y.Data).ToDictionary(y => getType(y.Key), y => y.ToList())
                        });
                    messagingContacts.AddRange(dataFromContacts);
                }

                OnSendBeforeExecution(subscriptionInstance, parameters, messagingContacts);

                foreach (var pair in subscriptionInstance._services)
                {
                    try
                    {
                        var messagingService = AppCore.Get<IMessagingService>(pair.Key);
                        if (messagingService == null)
                        {
                            var msg = $"Рассылка: '{subscriptionInstance.SubscriptionDescription.Id}' / '{subscriptionInstance.SubscriptionDescription.Name}'.\r\n";
                            msg += $"Коннектор: '{pair.Key.FullName}' / '{pair.Value.GetType().FullName}'.";
                            this.RegisterEvent(Journaling.EventType.Error, "Ошибка поиска сервиса отправки/получения сообщений для рассылки", msg);
                        }
                        pair.Value(messagingService, subscriptionInstance.SubscriptionDescription, parameters, messagingContacts);
                    }
                    catch (Exception ex)
                    {
                        var msg = $"Рассылка: '{subscriptionInstance.SubscriptionDescription.Id}' / '{subscriptionInstance.SubscriptionDescription.Name}'.\r\n";
                        msg += $"Коннектор: '{pair.Key.FullName}' / '{pair.Value.GetType().FullName}'.";
                        this.RegisterEvent(Journaling.EventType.Error, "Неожиданная ошибка рассылки", msg, ex);
                    }
                }
                return new ExecutionResult<Exception>(true);
            }
            catch (Exception ex)
            {
                var msg = $"Рассылка: '{subscriptionInstance.SubscriptionDescription.Id}' / '{subscriptionInstance.SubscriptionDescription.Name}'.\r\n";
                this.RegisterEvent(Journaling.EventType.Error, "Неожиданная ошибка рассылки", msg, ex);
                return new ExecutionResult<Exception>(false, "Неожиданная ошибка рассылки", ex);
            }
        }

        /// <summary>
        /// Вызывается перед началом выполнения команд в рассылке <typeparamref name="TSubscription"/>.
        /// </summary>
        /// <param name="subscription">Экземпляр объекта, олицетворяющего подписку.</param>
        /// <param name="parameters">Набор параметров для конкретного вызова подписки.</param>
        /// <param name="messagingContacts">Список подписчиков-получателей информации.</param>
        /// <seealso cref="SubscriptionParametrizedCall{TParameters}.ExecuteSubscription{TSubscription}"/>
        /// <seealso cref="AsParametrized{TParameters}(TParameters)"/>
        protected virtual void OnSendBeforeExecution<TSubscription, TParameters>(TSubscription subscription, TParameters parameters, MessagingContacts messagingContacts)
            where TSubscription : SubscriptionBase<TSubscription, TParameters>
        {
        }
        #endregion

        #region Свойства
        /// <summary>
        /// Описание группы подписок "Система".
        /// </summary>
        public SubscriptionGroupDescription SubscriptionGroupSystem { get; private set; }
        #endregion
    }

    /// <summary>
    /// Подготовленный запрос с параметрами выполнения для вызова подписки.
    /// </summary>
    /// <typeparam name="TParameters"></typeparam>
    public class SubscriptionParametrizedCall<TParameters>
    {
        internal SubscriptionsManager _manager;
        internal TParameters _parameters;

        /// <summary>
        /// Выполняет набор команд, заданный для подписки <typeparamref name="TSubscription"/>.
        /// </summary>
        public void ExecuteSubscription<TSubscription>()
            where TSubscription : SubscriptionBase<TSubscription, TParameters>
        {
            try
            {
                _manager.ExecuteSubscription<TSubscription, TParameters>(_parameters);
            }
            finally
            {
                _manager = null;
                _parameters = default(TParameters);
            }
        }
    }
}
