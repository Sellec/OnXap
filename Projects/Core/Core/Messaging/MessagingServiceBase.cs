using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

#pragma warning disable CS0618

namespace OnXap.Messaging
{
    using Components;
    using Core;
    using Core.Items;
    using Messages;
    using ServiceMonitor;
    using TaskSheduling;

    /// <summary>
    /// Предпочтительная базовая реализация сервиса обработки сообщений для приложения.
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения, с которым работает сервис.</typeparam>
    public abstract class MessagingServiceBase<TMessage> : 
        CoreComponentBase,
        IMessagingService,
        IMessagingServiceInternal,
        IAutoStart
        where TMessage : MessageBase, new()
    {
        class QueuePositionInfo
        {
            public int IdQueueCurrent;
            public int IdQueueMax;
        }

        private readonly string TasksOutcomingSend;
        private readonly string TasksIncomingReceive;
        private readonly string TasksIncomingHandle;

        private Types.ConcurrentFlagLocker<string> _executingFlags = new Types.ConcurrentFlagLocker<string>();
        private QueuePositionInfo _outcomingQueueInfo = new QueuePositionInfo();
        private QueuePositionInfo _incomingHandleQueueInfo = new QueuePositionInfo();
        private TaskDescription _taskOutcomingMessages = null;

        private const int JournalEventBase = 10000;

        /// <summary>
        /// Код события ошибки преобразования тела сообщения.
        /// </summary>
        public const int JournalEventErrorBodyConverting = JournalEventBase + 1;

        /// <summary>
        /// Создает новый экземпляр сервиса.
        /// </summary>
        /// <param name="serviceName">Текстовое название сервиса.</param>
        /// <param name="serviceID">Уникальный идентификатор сервиса.</param>
        protected MessagingServiceBase(string serviceName, Guid serviceID)
        {
            var type = GetType();
            TasksOutcomingSend = type.FullName + "_" + nameof(TasksOutcomingSend);
            TasksIncomingReceive = type.FullName + "_" + nameof(TasksIncomingReceive);
            TasksIncomingHandle = type.FullName + "_" + nameof(TasksIncomingHandle);

            if (string.IsNullOrEmpty(serviceName)) throw new ArgumentNullException(nameof(serviceName));
            if (serviceID == null) throw new ArgumentNullException(nameof(serviceID));

            ServiceID = serviceID;
            ServiceName = serviceName;

            IdMessageType = ItemTypeFactory.GetItemType(typeof(TMessage)).IdItemType;
        }

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStarting()
        {
            AppCore.Get<Journaling.JournalingManager>().RegisterJournalTypedInternal(GetType(), ServiceName, null);
            this.RegisterServiceState(ServiceStatus.RunningIdeal, "Сервис запущен.");

            var type = GetType();

            var taskSchedulingManager = AppCore.Get<TaskSchedulingManager>();
            taskSchedulingManager.RegisterTask(new TaskRequest()
            {
                Name = $"Сервис '{GetType().FullName}': прием входящих сообщений",
                Description = "",
                IsEnabled = true,
                TaskOptions = TaskOptions.PreventParallelExecution,
                UniqueKey = "CallServiceIncomingReceive_" + GetType().FullName,
                ExecutionLambda = () => MessagingManager.CallServiceIncomingReceive(type, TimeSpan.FromMinutes(4)),
                Schedules = new List<TaskSchedule>() { new TaskCronSchedule(Cron.MinuteInterval(1)) },
                JournalOptions = new Journaling.JournalOptions() { LimitByLastNDays = 0 }
            });
            taskSchedulingManager.RegisterTask(new TaskRequest()
            {
                Name = $"Сервис '{GetType().FullName}': обработка входящих сообщений",
                Description = "",
                IsEnabled = true,
                TaskOptions = TaskOptions.PreventParallelExecution,
                UniqueKey = "CallServiceIncomingHandle_" + GetType().FullName,
                ExecutionLambda = () => MessagingManager.CallServiceIncomingHandle(type, TimeSpan.FromMinutes(4)),
                Schedules = new List<TaskSchedule>() { new TaskCronSchedule(Cron.MinuteInterval(1)) },
                JournalOptions = new Journaling.JournalOptions() { LimitByLastNDays = 0 }
            });
            _taskOutcomingMessages = taskSchedulingManager.RegisterTask(new TaskRequest()
            {
                Name = $"Сервис '{GetType().FullName}': отправка исходящих сообщений",
                Description = "",
                IsEnabled = true,
                TaskOptions = TaskOptions.PreventParallelExecution,
                UniqueKey = "CallServiceOutcoming_" + GetType().FullName,
                ExecutionLambda = () => MessagingManager.CallServiceOutcoming(type, TimeSpan.FromMinutes(4)),
                Schedules = new List<TaskSchedule>() { new TaskCronSchedule(Cron.MinuteInterval(1)) },
                JournalOptions = new Journaling.JournalOptions() { LimitByLastNDays = 0 }
            });

            OnServiceStarting();
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStarted()
        {
            OnServiceStarted();
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
            this.RegisterServiceState(ServiceStatus.Shutdown, "Сервис остановлен.");

            var type = GetType();

            _taskOutcomingMessages = null;
            OnServiceStop();
        }

        /// <summary>
        /// Вызывается при запуске сервиса.
        /// </summary>
        protected virtual void OnServiceStarting()
        {
        }

        /// <summary>
        /// Вызывается после запуска сервиса.
        /// </summary>
        protected virtual void OnServiceStarted()
        {
        }

        /// <summary>
        /// Вызывается при остановке сервиса.
        /// </summary>
        protected virtual void OnServiceStop()
        {
        }
        #endregion

        #region Сообщения
        /// <summary>
        /// Регистрирует сообщение <paramref name="message"/> в очередь на отправку.
        /// </summary>
        /// <returns>Возвращает true в случае успеха и false в случае ошибки во время регистрации сообщения.</returns>
        [ApiReversible]
        protected bool RegisterOutcomingMessage(TMessage message, out MessageInfo<TMessage> messageInfo)
        {
            try
            {
                var resolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                resolver.DefaultMembersSearchFlags = resolver.DefaultMembersSearchFlags | System.Reflection.BindingFlags.NonPublic;

                using (var db = new DB.DataContext())
                {
                    var mess = new DB.MessageQueue()
                    {
                        IdMessageType = IdMessageType,
                        StateType = DB.MessageStateType.NotProcessed,
                        Direction = false,
                        DateCreate = DateTime.Now,
                        MessageInfo = Newtonsoft.Json.JsonConvert.SerializeObject(message, new Newtonsoft.Json.JsonSerializerSettings() { ContractResolver = resolver }),
                    };

                    db.MessageQueue.Add(mess);
                    db.SaveChanges();

                    var taskOutcomingMessages = _taskOutcomingMessages;
                    if (taskOutcomingMessages != null) AppCore.Get<TaskSchedulingManager>()?.ExecuteTask(taskOutcomingMessages);

                    messageInfo = new MessageInfo<TMessage>(new IntermediateStateMessage<TMessage>(message, mess));
                    return true;
                }
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.Error, "Ошибка регистрации исходящего сообщения", null, ex);
                messageInfo = null;
                return false;
            }
        }

        /// <summary>
        /// Регистрирует сообщение <paramref name="message"/> как входящее. Оно поступит в обработку в компоненты <see cref="IncomingMessageHandler{TMessage}"/>.
        /// </summary>
        /// <returns>Возвращает true в случае успеха и false в случае ошибки во время регистрации сообщения.</returns>
        [ApiReversible]
        protected bool RegisterIncomingMessage(TMessage message)
        {
            try
            {
                var resolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                resolver.DefaultMembersSearchFlags = resolver.DefaultMembersSearchFlags | System.Reflection.BindingFlags.NonPublic;

                using (var db = new DB.DataContext())
                {
                    var mess = new DB.MessageQueue()
                    {
                        IdMessageType = IdMessageType,
                        StateType = DB.MessageStateType.NotProcessed,
                        Direction = true,
                        DateCreate = DateTime.Now,
                        MessageInfo = Newtonsoft.Json.JsonConvert.SerializeObject(message, new Newtonsoft.Json.JsonSerializerSettings() { ContractResolver = resolver }),
                    };

                    db.MessageQueue.Add(mess);
                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.Error, "Ошибка регистрации входящего сообщения", null, ex);
                return false;
            }
        }

        /// <summary>
        /// Возвращает информацию о сообщении с указанным идентификатором.
        /// </summary>
        /// <returns>
        /// Если тип сообщения, на которое ссылается идентификатор <paramref name="idMessage"/>, не соответствует типу <typeparamref name="TMessage"/>, возвращает null.
        /// Если сообщение с указанным идентификатором не найдено, возвращает null.
        /// </returns>
        [ApiReversible]
        public MessageInfo<TMessage> GetMessageById(int idMessage)
        {
            using (var db = new DB.DataContext())
            {
                var message = db.MessageQueue.Where(x => x.IdQueue == idMessage).FirstOrDefault();
                if (message == null || message.IdMessageType != IdMessageType) return null;

                var resolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                resolver.DefaultMembersSearchFlags = resolver.DefaultMembersSearchFlags | System.Reflection.BindingFlags.NonPublic;

                try
                {
                    var str = message.MessageInfo;
                    return new MessageInfo<TMessage>(
                        new IntermediateStateMessage<TMessage>(
                            Newtonsoft.Json.JsonConvert.DeserializeObject<TMessage>(
                                str, 
                                new Newtonsoft.Json.JsonSerializerSettings(){ ContractResolver = resolver }
                            ), 
                            message
                        )
                    );
                }
                catch (Exception ex)
                {
                    var messageInfo = new MessageInfo<TMessage>(new IntermediateStateMessage<TMessage>(new TMessage(), message));
                    this.RegisterEventForItem(new ItemKey(IdMessageType, idMessage), Journaling.EventType.Error, JournalEventErrorBodyConverting, "Ошибка преобразования тела сообщения", null, ex);
                    return messageInfo;
                }
            }
        }

        private List<IntermediateStateMessage<TMessage>> GetMessages(DB.DataContext db, bool direction, int limit, QueuePositionInfo queuePositionInfo)
        {
            var dateTime = DateTime.Now;
            var query = db.MessageQueue.
                Where(x =>
                    x.Direction == direction &&
                    x.IdMessageType == IdMessageType &&
                    (x.StateType == DB.MessageStateType.NotProcessed || x.StateType == DB.MessageStateType.Repeat) &&
                    (!x.DateDelayed.HasValue || x.DateDelayed.Value <= dateTime)
                ).
                OrderBy(x => x.IdQueue);

            if (queuePositionInfo.IdQueueMax <= 0)
            {
                var idQueueMax = query.Max(x => (int?)x.IdQueue) ?? 0;
                queuePositionInfo.IdQueueCurrent = 0;
                queuePositionInfo.IdQueueMax = idQueueMax;
            }
            else
            {
                query = query.
                    Where(x => x.IdQueue > queuePositionInfo.IdQueueCurrent && x.IdQueue <= queuePositionInfo.IdQueueMax).
                    OrderBy(x => x.IdQueue);
            }

            var messages = query.Take(limit).ToList();
            if (messages.Count == 0)
            {
                queuePositionInfo.IdQueueMax = 0;
                return new List<IntermediateStateMessage<TMessage>>();
            }

            var resolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
            resolver.DefaultMembersSearchFlags = resolver.DefaultMembersSearchFlags | System.Reflection.BindingFlags.NonPublic;

            var messagesUnserialized = messages.Select(x =>
            {
                try
                {
                    var str = x.MessageInfo;
                    return new IntermediateStateMessage<TMessage>(
                        Newtonsoft.Json.JsonConvert.DeserializeObject<TMessage>(
                            str,
                            new Newtonsoft.Json.JsonSerializerSettings() { ContractResolver = resolver }
                        ),
                        x
                    );
                }
                catch (Exception ex)
                {
                    var intermediateMessage = new IntermediateStateMessage<TMessage>(null, x);
                    intermediateMessage.MessageSource.StateType = DB.MessageStateType.Error;
                    intermediateMessage.MessageSource.State = ex.Message;
                    intermediateMessage.MessageSource.DateChange = DateTime.Now;
                    return intermediateMessage;
                }
            }).ToList();

            return messagesUnserialized;
        }
        #endregion

        #region Методы
        /// <summary>
        /// Возвращает список активных компонентов, работающих с типом сообщений сервиса.
        /// </summary>
        /// <seealso cref="Core.Configuration.CoreConfiguration.MessagingServicesComponentsSettings"/>
        protected List<MessagingServiceComponent<TMessage>> GetComponents()
        {
            return AppCore.Get<MessagingManager>().GetComponentsByMessageType<TMessage>().ToList();
        }

        /// <summary>
        /// Возвращает количество неотправленных сообщений, с которыми работает сервис.
        /// </summary>
        /// <returns></returns>
        [ApiReversible]
        public virtual int GetOutcomingQueueLength()
        {
            using (var db = new DB.DataContext())
            {
                return db.MessageQueue.Where(x => x.IdMessageType == IdMessageType && (x.StateType == DB.MessageStateType.NotProcessed || x.StateType == DB.MessageStateType.Repeat)).Count();
            }
        }
        #endregion

        #region IInternalForTasks
        void IMessagingServiceInternal.PrepareOutcoming(TimeSpan executeInterval)
        {
            if (AppCore.GetState() != CoreComponentState.Started) return;

            var type = GetType();

            if (!_executingFlags.TryLock(TasksOutcomingSend)) return;
            _executingFlags.ReleaseLock(nameof(RegisterOutcomingMessage));

            int messagesAll = 0;
            int messagesSent = 0;
            int messagesErrors = 0;

            try
            {
                using (var db = new DB.DataContext())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress)) // Здесь Suppress вместо RequiresNew, т.к. весь процесс отправки занимает много времени и блокировать таблицу нельзя.
                {
                    var timeEnd = DateTimeOffset.Now.Add(executeInterval);
                    while (DateTimeOffset.Now < timeEnd)
                    {
                        var messages = GetMessages(db, false, 100, _outcomingQueueInfo);
                        if (messages.IsNullOrEmpty()) break;
                        messagesAll += messages.Count;

                        OnBeforeExecuteOutcoming(messagesAll);

                        var processedMessages = new List<IntermediateStateMessage<TMessage>>();

                        var time = new MeasureTime();
                        foreach (var intermediateMessage in messages)
                        {
                            if (DateTimeOffset.Now >= timeEnd) break;

                            if (intermediateMessage.MessageSource.StateType == DB.MessageStateType.Error)
                            {
                                processedMessages.Add(intermediateMessage);
                                continue;
                            }

                            var components = GetComponents().
                                OfType<OutcomingMessageSender<TMessage>>().
                                Select(x => new
                                {
                                    Component = x,
                                    IdTypeComponent = ItemTypeFactory.GetItemType(x.GetType())?.IdItemType
                                }).
                                OrderBy(x => ((IPoolObjectOrdered)x.Component).OrderInPool).
                                ToList();

                            if (intermediateMessage.MessageSource.IdTypeComponent.HasValue)
                            {
                                components = components.Where(x => x.IdTypeComponent.HasValue && x.IdTypeComponent == intermediateMessage.MessageSource.IdTypeComponent).ToList();
                            }

                            foreach (var componentInfo in components)
                            {
                                try
                                {
                                    var component = componentInfo.Component;
                                    var messageInfo = new MessageInfo<TMessage>(intermediateMessage);
                                    var componentResult = component.OnSend(messageInfo, this);
                                    if (componentResult != null)
                                    {
                                        intermediateMessage.MessageSource.DateChange = DateTime.Now;
                                        switch (componentResult.StateType)
                                        {
                                            case MessageStateType.Completed:
                                                intermediateMessage.MessageSource.StateType = DB.MessageStateType.Complete;
                                                intermediateMessage.MessageSource.State = null;
                                                intermediateMessage.MessageSource.IdTypeComponent = null;
                                                intermediateMessage.MessageSource.DateDelayed = null;
                                                break;

                                            case MessageStateType.Delayed:
                                                intermediateMessage.MessageSource.StateType = DB.MessageStateType.NotProcessed;
                                                intermediateMessage.MessageSource.State = componentResult.State;
                                                intermediateMessage.MessageSource.IdTypeComponent = null;
                                                intermediateMessage.MessageSource.DateDelayed = componentResult.DateDelayed;
                                                break;

                                            case MessageStateType.Repeat:
                                                intermediateMessage.MessageSource.StateType = DB.MessageStateType.Repeat;
                                                intermediateMessage.MessageSource.State = componentResult.State;
                                                intermediateMessage.MessageSource.IdTypeComponent = componentInfo.IdTypeComponent;
                                                intermediateMessage.MessageSource.DateDelayed = componentResult.DateDelayed;
                                                break;

                                            case MessageStateType.Error:
                                                intermediateMessage.MessageSource.StateType = DB.MessageStateType.Error;
                                                intermediateMessage.MessageSource.State = componentResult.State;
                                                intermediateMessage.MessageSource.IdTypeComponent = null;
                                                intermediateMessage.MessageSource.DateDelayed = componentResult.DateDelayed;
                                                break;

                                        }
                                        messagesSent++;
                                        processedMessages.Add(intermediateMessage);
                                        break;
                                    }
                                }
                                catch
                                {
                                    messagesErrors++;
                                    continue;
                                }
                            }

                            _outcomingQueueInfo.IdQueueCurrent = intermediateMessage.MessageSource.IdQueue;

                            if (time.Calculate(false).TotalSeconds >= 3)
                            {
                                db.SaveChanges();
                                processedMessages.Clear();
                                time.Start();
                            }
                        }

                        if (processedMessages.Count > 0)
                        {
                            db.SaveChanges();
                        }
                    }

                    db.SaveChanges();
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                this.RegisterServiceState(ServiceStatus.RunningWithErrors, $"Сообщений получено для отправки - {messagesAll}. Отправлено - {messagesSent}. Ошибки отправки - {messagesErrors}.", ex);
            }
            finally
            {
                _executingFlags.ReleaseLock(TasksOutcomingSend);
            }
        }

        void IMessagingServiceInternal.PrepareIncomingReceive(TimeSpan executeInterval)
        {
            if (AppCore.GetState() != CoreComponentState.Started) return;

            var type = GetType();

            if (!_executingFlags.TryLock(TasksIncomingReceive)) return;

            int messagesReceived = 0;

            try
            {
                using (var db = new DB.DataContext())
                {
                    var components = GetComponents().
                    OfType<IncomingMessageReceiver<TMessage>>().
                    Select(x => new
                    {
                        Component = x,
                        IdTypeComponent = ItemTypeFactory.GetItemType(x.GetType())?.IdItemType
                    }).
                    OrderBy(x => ((IPoolObjectOrdered)x.Component).OrderInPool).
                    ToList();

                    var timeEnd = DateTimeOffset.Now.Add(executeInterval);

                    using (var scope = db.CreateScope(TransactionScopeOption.Suppress)) // Здесь Suppress вместо RequiresNew, т.к. весь процесс отправки занимает много времени и блокировать таблицу нельзя.
                    {
                        foreach (var componentInfo in components)
                        {
                            try
                            {
                                var messages = componentInfo.Component.OnReceive(this);
                                if (messages != null && messages.Count > 0)
                                {
                                    int countAdded = 0;
                                    foreach (var message in messages)
                                    {
                                        if (message == null) continue;

                                        var stateType = DB.MessageStateType.NotProcessed;
                                        if (message.IsComplete) stateType = DB.MessageStateType.Complete;
                                        if (message.IsError) stateType = DB.MessageStateType.Error;

                                        var mess = new DB.MessageQueue()
                                        {
                                            IdMessageType = IdMessageType,
                                            Direction = true,
                                            State = message.State,
                                            StateType = stateType,
                                            DateCreate = DateTime.Now,
                                            DateDelayed = message.DateDelayed,
                                            MessageInfo = Newtonsoft.Json.JsonConvert.SerializeObject(message.Message),
                                        };

                                        db.MessageQueue.Add(mess);
                                        countAdded++;
                                        messagesReceived++;

                                        if (countAdded >= 50)
                                        {
                                            db.SaveChanges();
                                            countAdded = 0;
                                        }
                                    }

                                    db.SaveChanges();
                                }
                            }
                            catch (Exception ex)
                            {
                                this.RegisterServiceEvent(Journaling.EventType.Error, $"Ошибка вызова '{nameof(componentInfo.Component.OnReceive)}'", $"Ошибка вызова '{nameof(componentInfo.Component.OnReceive)}' для компонента '{componentInfo?.Component?.GetType()?.FullName}'.", ex);
                                continue;
                            }

                            try
                            {
                                while (true)
                                {
                                    var message = componentInfo.Component.OnBeginReceive(this);
                                    if (message == null) break;

                                    DB.MessageQueue queueMessage = null;

                                    var queueState = DB.MessageStateType.NotProcessed;
                                    if (message.IsComplete) queueState = DB.MessageStateType.Complete;
                                    if (message.IsError) queueState = DB.MessageStateType.Error;

                                    try
                                    {
                                        var mess = new DB.MessageQueue()
                                        {
                                            IdMessageType = IdMessageType,
                                            Direction = true,
                                            State = message.State,
                                            StateType = DB.MessageStateType.IntermediateAdded,
                                            DateCreate = DateTime.Now,
                                            DateDelayed = message.DateDelayed,
                                            MessageInfo = Newtonsoft.Json.JsonConvert.SerializeObject(message.Message),
                                        };

                                        db.MessageQueue.Add(mess);
                                        db.SaveChanges();

                                        queueMessage = mess;
                                    }
                                    catch (Exception ex)
                                    {
                                        this.RegisterServiceEvent(Journaling.EventType.Error, $"Ошибка регистрации сообщения после '{nameof(componentInfo.Component.OnBeginReceive)}'", $"Ошибка регистрации сообщения после вызова '{nameof(componentInfo.Component.OnBeginReceive)}' для компонента '{componentInfo?.Component?.GetType()?.FullName}'.", ex);
                                        try
                                        {
                                            componentInfo.Component.OnEndReceive(false, message, this);
                                        }
                                        catch (Exception ex2)
                                        {
                                            this.RegisterServiceEvent(Journaling.EventType.Error, $"Ошибка вызова '{nameof(componentInfo.Component.OnBeginReceive)}'", $"Ошибка вызова '{nameof(componentInfo.Component.OnBeginReceive)}' для компонента '{componentInfo?.Component?.GetType()?.FullName}' после ошибки регистрации сообщения.", ex2);
                                        }
                                        continue;
                                    }

                                    try
                                    {
                                        var endReceiveResult = componentInfo.Component.OnEndReceive(true, message, this);
                                        if (endReceiveResult)
                                        {
                                            queueMessage.StateType = queueState;
                                        }
                                        else
                                        {
                                            db.MessageQueue.Remove(queueMessage);
                                        }
                                        db.SaveChanges();
                                    }
                                    catch (Exception ex)
                                    {
                                        this.RegisterServiceEvent(Journaling.EventType.Error, $"Ошибка вызова '{nameof(componentInfo.Component.OnBeginReceive)}'", $"Ошибка вызова '{nameof(componentInfo.Component.OnBeginReceive)}' для компонента '{componentInfo?.Component?.GetType()?.FullName}' после успешной регистрации сообщения.", ex);
                                    }

                                    if (DateTimeOffset.Now >= timeEnd) break;
                                }
                            }
                            catch (Exception ex)
                            {
                                this.RegisterServiceEvent(Journaling.EventType.Error, $"Ошибка вызова '{nameof(componentInfo.Component.OnBeginReceive)}'", $"Ошибка вызова '{nameof(componentInfo.Component.OnBeginReceive)}' для компонента '{componentInfo?.Component?.GetType()?.FullName}'.", ex);
                                continue;
                            }
                        }
                        db.SaveChanges();
                        scope.Complete();
                    }
                }

                if (messagesReceived > 0)
                {
                    this.RegisterServiceState(ServiceStatus.RunningIdeal, $"Сообщений получено - {messagesReceived}.");
                }

                var service = AppCore.Get<Monitor>().GetService(ServiceID);
                if (service != null && (DateTime.Now - service.LastDateEvent).TotalHours >= 1)
                {
                    this.RegisterServiceState(ServiceStatus.RunningIdeal, $"Сообщений нет, сервис работает без ошибок.");
                }
            }
            catch (Exception ex)
            {
                this.RegisterServiceState(ServiceStatus.RunningWithErrors, $"Сообщений получено - {messagesReceived}.", ex);
            }
            finally
            {
                _executingFlags.ReleaseLock(TasksIncomingReceive);
            }
        }

        void IMessagingServiceInternal.PrepareIncomingHandle(TimeSpan executeInterval)
        {
            if (AppCore.GetState() != CoreComponentState.Started) return;

            var type = GetType();

            if (!_executingFlags.TryLock(TasksIncomingHandle)) return;
            _executingFlags.ReleaseLock(nameof(RegisterIncomingMessage));

            int messagesAll = 0;
            int messagesHandled = 0;
            int messagesErrors = 0;

            try
            {
                using (var db = new DB.DataContext())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress))
                {
                    var timeEnd = DateTimeOffset.Now.Add(executeInterval);
                    while (DateTimeOffset.Now < timeEnd)
                    {
                        var messages = GetMessages(db, true, 100, _incomingHandleQueueInfo);
                        if (messages.IsNullOrEmpty()) break;

                        messagesAll += messages.Count;

                        var components = GetComponents().
                            OfType<IncomingMessageHandler<TMessage>>().
                            Select(x => new
                            {
                                Component = x,
                                IdTypeComponent = ItemTypeFactory.GetItemType(x.GetType())?.IdItemType
                            }).
                            OrderBy(x => ((IPoolObjectOrdered)x.Component).OrderInPool).
                            ToList();

                        foreach (var intermediateMessage in messages)
                        {
                            if (DateTimeOffset.Now >= timeEnd) break;

                            var componentsForMessage = components;
                            if (intermediateMessage.MessageSource.IdTypeComponent.HasValue)
                            {
                                components = components.Where(x => x.IdTypeComponent.HasValue && x.IdTypeComponent == intermediateMessage.MessageSource.IdTypeComponent).ToList();
                            }

                            foreach (var componentInfo in components)
                            {
                                try
                                {
                                    var component = componentInfo.Component;
                                    var messageInfo = new MessageInfo<TMessage>(intermediateMessage);
                                    var componentResult = component.OnPrepare(messageInfo, this);
                                    if (componentResult != null)
                                    {
                                        intermediateMessage.MessageSource.DateChange = DateTime.Now;
                                        switch (componentResult.StateType)
                                        {
                                            case MessageStateType.Completed:
                                                intermediateMessage.MessageSource.StateType = DB.MessageStateType.Complete;
                                                intermediateMessage.MessageSource.State = null;
                                                intermediateMessage.MessageSource.IdTypeComponent = null;
                                                intermediateMessage.MessageSource.DateDelayed = null;
                                                break;

                                            case MessageStateType.Delayed:
                                                intermediateMessage.MessageSource.StateType = DB.MessageStateType.NotProcessed;
                                                intermediateMessage.MessageSource.State = componentResult.State;
                                                intermediateMessage.MessageSource.IdTypeComponent = null;
                                                intermediateMessage.MessageSource.DateDelayed = componentResult.DateDelayed;
                                                break;

                                            case MessageStateType.Repeat:
                                                intermediateMessage.MessageSource.StateType = DB.MessageStateType.Repeat;
                                                intermediateMessage.MessageSource.State = componentResult.State;
                                                intermediateMessage.MessageSource.IdTypeComponent = componentInfo.IdTypeComponent;
                                                intermediateMessage.MessageSource.DateDelayed = componentResult.DateDelayed;
                                                break;

                                            case MessageStateType.Error:
                                                intermediateMessage.MessageSource.StateType = DB.MessageStateType.Error;
                                                intermediateMessage.MessageSource.State = componentResult.State;
                                                intermediateMessage.MessageSource.IdTypeComponent = null;
                                                intermediateMessage.MessageSource.DateDelayed = null;
                                                break;
                                        }
                                        messagesHandled++;
                                        db.SaveChanges();
                                        break;
                                    }
                                }
                                catch
                                {
                                    messagesErrors++;
                                    continue;
                                }
                            }

                            _incomingHandleQueueInfo.IdQueueCurrent = intermediateMessage.MessageSource.IdQueue;
                        }
                    }
                    db.SaveChanges();
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                this.RegisterServiceState(ServiceStatus.RunningWithErrors, $"Сообщений получено для обработки - {messagesAll}. Обработано - {messagesHandled}. Ошибки обработки - {messagesErrors}.", ex);
            }
            finally
            {
                _executingFlags.ReleaseLock(TasksIncomingHandle);
            }
        }

        #endregion

        #region Фоновые операции
        /// <summary>
        /// Вызывается перед началом отправки сообщений.
        /// </summary>
        protected virtual void OnBeforeExecuteOutcoming(int messagesCount)
        {

        }
        #endregion

        #region Свойства
        /// <summary>
        /// Возвращает идентификатор типа сообщения.
        /// </summary>
        public int IdMessageType { get; }

        /// <summary>
        /// Возвращает идентификатор сервиса.
        /// </summary>
        public Guid ServiceID
        {
            get;
            private set;
        }

        /// <summary>
        /// Возвращает название сервиса.
        /// </summary>
        public string ServiceName
        {
            get;
            private set;
        }

        #region IMonitoredService
        /// <summary>
        /// См. <see cref="IMonitoredService.ServiceStatus"/>.
        /// </summary>
        public virtual ServiceStatus ServiceStatus
        {
            get;
            protected set;
        }

        /// <summary>
        /// См. <see cref="IMonitoredService.ServiceStatusDetailed"/>.
        /// </summary>
        public virtual string ServiceStatusDetailed
        {
            get;
            protected set;
        }

        /// <summary>
        /// См. <see cref="IMonitoredService.IsSupportsCurrentStatusInfo"/>.
        /// </summary>
        public virtual bool IsSupportsCurrentStatusInfo
        {
            get;
            protected set;
        }
        #endregion

        #region IMessagingServiceBackgroundOperations
        /// <summary>
        /// Указывает, что сервис поддерживает прием сообщений.
        /// </summary>
        public virtual bool IsSupportsIncoming
        {
            get;
            protected set;
        }

        /// <summary>
        /// Указывает, что сервис поддерживает отправку сообщений.
        /// </summary>
        public virtual bool IsSupportsOutcoming
        {
            get;
            protected set;
        }

        /// <summary>
        /// Возвращает длину очереди на отправку сообщений.
        /// </summary>
        public virtual int OutcomingQueueLength
        {
            get;
            protected set;
        }
        #endregion
        #endregion
    }
}
