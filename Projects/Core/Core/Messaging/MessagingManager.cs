using OnUtils;
using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OnXap.Messaging
{
    using Components;
    using Core;
    using Journaling;
    using Messages;
    using OnUtils.Types;
    using TaskSheduling;
    using ExecutionResultMessagingServiceOptions = ExecutionResult<MessagingServiceOptions>;

    /// <summary>
    /// Представляет менеджер, управляющий обменом сообщениями - уведомления, электронная почта, смс и прочее.
    /// </summary>
    public sealed class MessagingManager :
        CoreComponentBase,
        IComponentSingleton,
        IAutoStart,
        ITypedJournalComponent<MessagingManager>
    {
        class InstanceActivatingHandlerImpl : IInstanceActivatingHandler
        {
            private readonly MessagingManager _manager;

            public InstanceActivatingHandlerImpl(MessagingManager manager)
            {
                _manager = manager;
            }

            void IInstanceActivatingHandler.OnInstanceActivating<TRequestedType>(object instance)
            {
                if (instance is IMessagingServiceInternal service)
                {
                    if (!_manager._services.Contains(service)) _manager._services.Add(service);
                }
            }
        }

        private static OnXApplication _appCore = null;

        static internal readonly DateTime DateDelayedEmpty = new DateTime(1900, 1, 1, 0, 0, 0);
        private readonly InstanceActivatingHandlerImpl _instanceActivatingHandler = null;
        private List<IMessagingServiceInternal> _services = new List<IMessagingServiceInternal>();

        private object _activeComponentsSyncRoot = new object();
        private List<IComponentTransient> _activeComponents = null;
        private List<IComponentTransient> _registeredComponents = null;

        private MessagingServiceOptions _messagingServiceOptionsDefault = new MessagingServiceOptions();
        private ConcurrentDictionary<int, MessagingServiceOptions> _messagingServiceOptions = new ConcurrentDictionary<int, MessagingServiceOptions>();
        private TaskDescription _taskDescriptionClear = null;

        /// <summary>
        /// </summary>
        public MessagingManager()
        {
            _instanceActivatingHandler = new InstanceActivatingHandlerImpl(this);
            _registeredComponents = new List<IComponentTransient>();
        }

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStarting()
        {
            this.RegisterJournal("Менеджер сообщений");

            _appCore = AppCore;
            AppCore.ObjectProvider.RegisterInstanceActivatingHandler(_instanceActivatingHandler);

        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStarted()
        {
            _taskDescriptionClear = AppCore.Get<TaskSchedulingManager>().RegisterTask(new TaskRequest()
            {
                Name = "Менеджер сообщений: очистка старых записей",
                IsEnabled = true,
                UniqueKey = typeof(MessagingManager).FullName + "_ClearLastNDays",
                Schedules = new List<TaskSchedule>()
                {
                    new TaskCronSchedule(Cron.Hourly()) { IsEnabled = true }
                },
                ExecutionLambda = task => ClearLastNDays(),
                TaskOptions = TaskOptions.AllowDisabling | TaskOptions.AllowManualSchedule | TaskOptions.PreventParallelExecution
            });

            // Попытка инициализировать все сервисы обработки сообщений, наследующиеся от IMessagingService.
            var types = AppCore.GetQueryTypes().Where(x => x.GetInterfaces().Contains(typeof(IMessagingServiceInternal))).ToList();
            foreach (var type in types)
            {
                try
                {
                    var instance = AppCore.Get<IMessagingServiceInternal>(type);
                    if (instance != null && !_services.Contains(instance)) _services.Add(instance);
                }
                catch { }
            }
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
        }
        #endregion

        #region Настройки журналов
        private void ClearLastNDays()
        {
            try
            {
                if (_taskDescriptionClear == null) return;

                foreach (var pair in _messagingServiceOptions)
                {
                    var lastNDaysValue = pair.Value.LimitByLastNDays ?? _messagingServiceOptionsDefault.LimitByLastNDays;
                    if (!lastNDaysValue.HasValue || lastNDaysValue.Value < 0) continue;

                    var dateLimit = DateTimeOffset.UtcNow.AddDays(-lastNDaysValue.Value);

                    using (var db = new DB.DataContext())
                    {
                        var hasRecords = true;
                        while (hasRecords)
                        {
                            for (int i = 0; i <= 2; i++)
                            {
                                try
                                {
                                    var rows = db.MessageQueue.Where(x => x.IdMessageType == pair.Key && x.DateCreate < dateLimit).OrderBy(x => x.DateCreate).Take(500).ToList();
                                    hasRecords = rows.Count > 0;
                                    if (hasRecords)
                                    {
                                        db.MessageQueue.RemoveRange(rows);
                                        db.SaveChanges();
                                    }
                                    break;
                                }
                                catch
                                {
                                    if (i == 2) hasRecords = false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.RegisterEvent(EventType.Error, "Ошибка очистки старых сообщений сервисов обработки сообщений", null, ex);
            }
        }

        #region Установка настроек
        /// <summary>
        /// Устанавливает свойства по-умолчанию для всех сервисов обработки сообщений, если для сервисов не заданы собственные значения свойств.
        /// </summary>
        /// <param name="messagingServiceOptions">Параметры сервисов обработки сообщений по-умолчанию.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="messagingServiceOptions"/> равен null.</exception>
        public void SetMessagingServiceOptionsDefault(MessagingServiceOptions messagingServiceOptions)
        {
            if (messagingServiceOptions == null) throw new ArgumentNullException(nameof(messagingServiceOptions));
            _messagingServiceOptionsDefault = messagingServiceOptions;
        }

        /// <summary>
        /// Устанавливает свойства сервиса обработки сообщений.
        /// </summary>
        /// <typeparam name="TMessagingServiceType">Тип сервиса обработки сообщений.</typeparam>
        /// <param name="messagingServiceOptions">Параметры сервиса обработки сообщений.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="messagingServiceOptions"/> равен null.</exception>
        /// <returns>Возвращает объект <see cref="ExecutionResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        public ExecutionResult SetMessagingServiceOptions<TMessagingServiceType>(MessagingServiceOptions messagingServiceOptions)
            where TMessagingServiceType : IMessagingService
        {
            if (!_services.Any(x => x.GetType() == typeof(TMessagingServiceType))) return new ExecutionResult(false, "Указанный сервис обработки сообщений не найден.");

            var type = TypeHelpers.ExtractGenericType(typeof(TMessagingServiceType), typeof(MessagingServiceBase<>));
            var messageType = Core.Items.ItemTypeFactory.GetItemType(type.GenericTypeArguments[0]);

            return SetMessagingServiceOptionsInternal(messageType.IdItemType, messagingServiceOptions);
        }

        internal ExecutionResult SetMessagingServiceOptionsInternal(int idMessageType, MessagingServiceOptions messagingServiceOptions)
        {
            if (messagingServiceOptions == null) throw new ArgumentNullException(nameof(messagingServiceOptions));
            _messagingServiceOptions[idMessageType] = messagingServiceOptions;
            return new ExecutionResult(true);
        }
        #endregion

        #region Получение настроек
        /// <summary>
        /// Возвращает свойства по-умолчанию для всех сервисов обработки сообщений.
        /// </summary>
        public MessagingServiceOptions GetMessagingServiceOptionsDefault()
        {
            return _messagingServiceOptionsDefault ?? new MessagingServiceOptions();
        }

        /// <summary>
        /// Возвращает свойства сервиса обработки сообщений.
        /// </summary>
        /// <typeparam name="TMessagingServiceType">Тип сервиса обработки сообщений.</typeparam>
        /// <returns>Возвращает объект <see cref="ExecutionResultMessagingServiceOptions"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        public ExecutionResultMessagingServiceOptions GetMessagingServiceOptions<TMessagingServiceType>()
            where TMessagingServiceType : IMessagingService
        {
            if (!_services.Any(x => x.GetType() == typeof(TMessagingServiceType))) return new ExecutionResultMessagingServiceOptions(false, "Указанный сервис обработки сообщений не найден.");

            var type = TypeHelpers.ExtractGenericType(typeof(TMessagingServiceType), typeof(MessagingServiceBase<>));
            var messageType = Core.Items.ItemTypeFactory.GetItemType(type.GenericTypeArguments[0]);

            return GetMessagingServiceOptionsInternal(messageType.IdItemType);
        }

        internal ExecutionResultMessagingServiceOptions GetMessagingServiceOptionsInternal(int idMessageType)
        {
            return new ExecutionResultMessagingServiceOptions(true, null, _messagingServiceOptions.TryGetValue(idMessageType, out var options) ? options : new MessagingServiceOptions());
        }
        #endregion
        #endregion

        #region Методы
        internal static void CallServiceIncomingHandle(Type serviceType, TimeSpan executeInterval)
        {
            var service = _appCore?.Get<MessagingManager>()?._services?.FirstOrDefault(x => x.GetType() == serviceType);
            service?.PrepareIncomingHandle(executeInterval);
        }

        internal static void CallServiceIncomingReceive(Type serviceType, TimeSpan executeInterval)
        {
            var service = _appCore?.Get<MessagingManager>()?._services?.FirstOrDefault(x => x.GetType() == serviceType);
            service?.PrepareIncomingReceive(executeInterval);
        }

        internal static void CallServiceOutcoming(Type serviceType, TimeSpan executeInterval)
        {
            var service = _appCore?.Get<MessagingManager>()?._services?.FirstOrDefault(x => x.GetType() == serviceType);
            service?.PrepareOutcoming(executeInterval);
        }

        /// <summary>
        /// Регистрирует новый компонент сервиса обработки сообщений.
        /// </summary>
        public void RegisterComponent<TMessage>(MessagingServiceComponent<TMessage> component)
            where TMessage : MessageBase, new()
        {
            if (component == null) return;
            if (_registeredComponents.Contains(component) || (_activeComponents != null && _activeComponents.Contains(component))) return;

            try
            {
                if (component.GetState() == CoreComponentState.None && component is IComponentStartable startableComponent)
                {
                    startableComponent.Start(AppCore);
                    var allowed = ((IInternal)component).OnStartComponent();
                    if (!allowed)
                    {
                        this.RegisterEvent(EventType.Error, "Отказ инициализации компонента", $"Компонент типа ('{component.GetType().FullName}') вернул отказ инициализации. См. журналы ошибок для поиска возможной информации.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                this.RegisterEvent(EventType.Error, "Ошибка запуска компонента", $"Во время инициализации компонента типа '{component.GetType().FullName}' возникла неожиданная ошибка.", null, ex.InnerException);
            }

            _registeredComponents.Add(component);
        }

        /// <summary>
        /// Возвращает список компонентов, поддерживающих обмен сообщениями указанного типа <typeparamref name="TMessage"/>.
        /// </summary>
        public IEnumerable<MessagingServiceComponent<TMessage>> GetComponentsByMessageType<TMessage>() where TMessage : MessageBase, new()
        {
            lock (_activeComponentsSyncRoot)
                if (_activeComponents == null)
                    UpdateComponentsFromSettings();

            var active = _activeComponents.OfType<MessagingServiceComponent<TMessage>>();
            var registered = _registeredComponents.OfType<MessagingServiceComponent<TMessage>>();

            return active.Union(registered);
        }

        /// <summary>
        /// Возвращает список сервисов обмена сообщениями.
        /// </summary>
        public IEnumerable<IMessagingService> GetMessagingServices()
        {
            return _services.OfType<IMessagingService>().ToList();
        }

        /// <summary>
        /// Пересоздает текущий используемый список компонентов с учетом настроек. Рекомендуется к использованию в случае изменения настроек.
        /// </summary>
        /// <see cref="Core.Configuration.CoreConfiguration.MessagingServicesComponentsSettings"/>
        public void UpdateComponentsFromSettings()
        {
            lock (_activeComponentsSyncRoot)
            {
                if (_activeComponents != null)
                    _activeComponents.ForEach(x =>
                    {
                        try
                        {
                            x.Stop();
                        }
                        catch (Exception ex)
                        {
                            this.RegisterEvent(EventType.Error, "Ошибка при закрытии компонента", $"Возникла ошибка при выгрузке компонента типа '{x.GetType().FullName}'.", null, ex);
                        }
                    });

                _activeComponents = new List<IComponentTransient>();

                var settings = AppCore.AppConfig.MessagingServicesComponentsSettings;
                if (settings != null)
                {
                    var types = AppCore.
                        GetQueryTypes().
                        Select(x => new { Type = x, Extracted = TypeHelpers.ExtractGenericType(x, typeof(MessagingServiceComponent<>)) }).
                        Where(x => x.Extracted != null).
                        Select(x => new { x.Type, MessageType = x.Extracted.GetGenericArguments()[0] }).
                        ToList();

                    foreach (var setting in settings)
                    {
                        var type = types.FirstOrDefault(x => x.Type.FullName == setting.TypeFullName);
                        if (type == null)
                        {
                            this.RegisterEvent(EventType.Error, "Ошибка при поиске компонента", $"Не найден тип компонента из настроек - '{setting.TypeFullName}'. Для стирания старых настроек следует зайти в настройку компонентов и сделать сохранение.");
                            continue;
                        }

                        try
                        {
                            var allowed = true;
                            var instance = AppCore.Create<IComponentTransient>(type.Type, component =>
                            {
                                ((IInternal)component).SerializedSettings = setting.SettingsSerialized;
                                allowed = ((IInternal)component).OnStartComponent();
                            });
                            if (allowed)
                            {
                                _activeComponents.Add(instance);
                            }
                            else
                            {
                                this.RegisterEvent(EventType.Error, "Отказ инициализации компонента", $"Компонент типа '{setting.TypeFullName}' ('{instance.GetType().FullName}') вернул отказ инициализации. См. журналы ошибок для поиска возможной информации.");
                            }
                        }
                        catch (Exception ex)
                        {
                            this.RegisterEvent(EventType.Error, "Ошибка создания компонента", $"Во время создания и инициализации компонента типа '{setting.TypeFullName}' возникла неожиданная ошибка.", null, ex.InnerException);
                        }
                    }
                }
            }
        }
        #endregion
    }
}

