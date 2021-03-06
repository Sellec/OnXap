﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OnXap.ServiceMonitor
{
    using Core;
    using Journaling;
    using Journaling.Model;

    /// <summary>
    /// Монитор сервисов. Позволяет вносить и получать информацию о сервисах, о их состоянии, событиях и пр.
    /// </summary>
    public class Monitor :
        CoreComponentBase,
        IComponentSingleton,
        ITypedJournalComponent<Monitor>
    {
        private static ConcurrentDictionary<Guid, JournalInfo> _servicesJournalsList = new ConcurrentDictionary<Guid, JournalInfo>();
        private static ConcurrentDictionary<Guid, ServiceInfo> _servicesList = new ConcurrentDictionary<Guid, ServiceInfo>();

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStarting()
        {
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
        }
        #endregion

        /// <summary>
        /// Фиксирует состояние сервиса без записи в журнал.
        /// </summary>
        /// <param name="service">Сервис, для которого производится регистрация состояния.</param>
        /// <param name="serviceStatus">Состояние сервиса.</param>
        /// <param name="serviceStatusDetailed">Детализированное состояние сервиса.</param>
        public void RegisterServiceStateWithoutJournal(IMonitoredService service, ServiceStatus serviceStatus, string serviceStatusDetailed = null)
        {
            var newState = new ServiceInfo()
            {
                ID = service.ServiceID,
                Name = service.ServiceName,
                LastStatus = serviceStatus,
                LastStatusDetailed = serviceStatusDetailed,
                LastDateEvent = DateTime.Now
            };

            _servicesList.AddOrUpdate(service.ServiceID, newState, (k, i) => newState);
        }

        /// <summary>
        /// Фиксирует состояние сервиса.
        /// </summary>
        /// <param name="service">Сервис, для которого производится регистрация состояния.</param>
        /// <param name="serviceStatus">Состояние сервиса.</param>
        /// <param name="serviceStatusDetailed">Детализированное состояние сервиса.</param>
        /// <param name="exception">Ошибки, если были зарегистрированы.</param>
        public void RegisterServiceState(IMonitoredService service, ServiceStatus serviceStatus, string serviceStatusDetailed = null, Exception exception = null)
        {
            RegisterServiceStateWithoutJournal(service, serviceStatus, serviceStatusDetailed);

            var eventType = EventType.Info;
            if (serviceStatus == ServiceStatus.RunningIdeal) eventType = EventType.Info;
            else if (serviceStatus == ServiceStatus.RunningWithErrors) eventType = EventType.Error;
            else if (serviceStatus == ServiceStatus.CannotRunBecouseOfErrors) eventType = EventType.CriticalError;
            else if (serviceStatus == ServiceStatus.Shutdown) eventType = EventType.Info;

            RegisterServiceEvent(service, eventType, serviceStatus.ToStringFriendly(), serviceStatusDetailed, exception);
        }

        /// <summary>
        /// Записывает в журнал сервиса событие, связанное с сервисом.
        /// </summary>
        /// <param name="service">Сервис, для которого производится регистрация состояния.</param>
        /// <param name="eventType">См. <see cref="JournalData.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalData.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalData.EventInfoDetailed"/>.</param>
        /// <param name="exception">См. <see cref="JournalData.ExceptionDetailed"/>.</param>
        public void RegisterServiceEvent(IMonitoredService service, EventType eventType, string eventInfo, string eventInfoDetailed = null, Exception exception = null)
        {
            var serviceJournal = GetJournalName(service);
            if (serviceJournal != null) AppCore.Get<JournalingManager>().RegisterEvent(serviceJournal.IdJournal, eventType, eventInfo, eventInfoDetailed, null, exception);
        }

        private JournalInfo GetJournalName(IMonitoredService service)
        {
            return _servicesJournalsList.GetOrAddWithExpiration(service.ServiceID, (key) =>
            {
                var result = AppCore.Get<JournalingManager>().RegisterJournal(1, service.ServiceName, "ServiceMonitor_" + key.ToString());
                if (!result.IsSuccess) this.RegisterEvent(EventType.Error, "Не удалось зарегистрировать журнал мониторинга", $"Журнал для типа '{service.GetType().FullName}'");
                return result.Result;
            }, TimeSpan.FromMinutes(15));
        }

        /// <summary>
        /// Возвращает журнал для указанного сервиса.
        /// </summary>
        public IEnumerable<JournalData> GetServiceJournal(IMonitoredService service)
        {
            var serviceJournal = GetJournalName(service);
            return GetServiceJournal(service.ServiceID);
        }

        /// <summary>
        /// Возвращает журнал для указанного идентификатора сервиса.
        /// </summary>
        public List<JournalData> GetServiceJournal(Guid serviceID)
        {
            if (_servicesJournalsList.TryGetValue(serviceID, out var serviceJournal))
            {
                var journalingManager = AppCore.Get<JournalingManager>();
                using (var db = new Journaling.DB.DataContext())
                {
                    var query = journalingManager.DatabaseAccessor.CreateQueryJournalData(db).
                        Where(x => x.JournalData.IdJournal == serviceJournal.IdJournal).
                        OrderByDescending(x => x.JournalData.DateEvent);

                    var data = journalingManager.DatabaseAccessor.FetchQueryJournalData(query);
                    return data;
                }
            }
            else
            {
                return new List<JournalData>();
            }
        }

        /// <summary>
        /// Возвращает список сервисов.
        /// </summary>
        public IDictionary<Guid, ServiceInfo> GetServicesList()
        {
            return _servicesList;
        }

        /// <summary>
        /// Возвращает сервис с указанным идентификатором.
        /// </summary>
        /// <returns>Объект с данными сервиса или null, если сервис не найден.</returns>
        public ServiceInfo GetService(Guid serviceID)
        {
            return _servicesList.TryGetValue(serviceID, out ServiceInfo serviceInfo) ? serviceInfo : null;
        }
    }
}

namespace System
{
    using OnXap.Journaling;
    using OnXap.Journaling.Model;
    using ServiceMonitor = OnXap.ServiceMonitor;

    /// <summary>
    /// </summary>
    public static class MonitorExtension
    {
        /// <summary>
        /// Фиксирует состояние сервиса на момент вызова метода без записи в журнал.
        /// </summary>
        /// <param name="service">Сервис, для которого производится регистрация состояния.</param>
        /// <param name="serviceStatus">Состояние сервиса.</param>
        /// <param name="serviceStatusDetailed">Детализированное состояние сервиса.</param>
        public static void RegisterServiceStateWithoutJournal(this ServiceMonitor.IMonitoredService service, ServiceMonitor.ServiceStatus serviceStatus, string serviceStatusDetailed = null)
        {
            service.GetAppCore().Get<ServiceMonitor.Monitor>()?.RegisterServiceStateWithoutJournal(service, serviceStatus, serviceStatusDetailed);
        }

        /// <summary>
        /// Фиксирует состояние сервиса на момент вызова метода.
        /// </summary>
        /// <param name="service">Сервис, для которого производится регистрация состояния.</param>
        /// <param name="serviceStatus">Состояние сервиса.</param>
        /// <param name="serviceStatusDetailed">Детализированное состояние сервиса.</param>
        /// <param name="exception">Ошибки, если были зарегистрированы.</param>
        public static void RegisterServiceState(this ServiceMonitor.IMonitoredService service, ServiceMonitor.ServiceStatus serviceStatus, string serviceStatusDetailed = null, Exception exception = null)
        {
            service.GetAppCore().Get<ServiceMonitor.Monitor>()?.RegisterServiceState(service, serviceStatus, serviceStatusDetailed, exception);
        }

        /// <summary>
        /// Записывает в журнал сервиса событие, связанное с сервисом.
        /// </summary>
        /// <param name="service">Сервис, для которого производится регистрация состояния.</param>
        /// <param name="eventType">См. <see cref="JournalData.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalData.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalData.EventInfoDetailed"/>.</param>
        /// <param name="exception">См. <see cref="JournalData.ExceptionDetailed"/>.</param>
        public static void RegisterServiceEvent(this ServiceMonitor.IMonitoredService service, EventType eventType, string eventInfo, string eventInfoDetailed = null, Exception exception = null)
        {
            service.GetAppCore().Get<ServiceMonitor.Monitor>()?.RegisterServiceEvent(service, eventType, eventInfo, eventInfoDetailed, exception);
        }

    }
}