﻿using System;
using System.Text;

namespace OnXap.ServiceMonitor
{
    using Core;

#pragma warning disable CS1591 // todo внести комментарии.
    public abstract class ServiceBase : CoreComponentBase, IMonitoredService
    {
        private Guid _serviceID = Guid.Empty;
        private string _serviceName = string.Empty;

        public ServiceBase()
        {
            if (string.IsNullOrEmpty(ServiceName)) throw new NullReferenceException($"Свойство {nameof(ServiceName)} не может быть пустым. Переопределите его в классе-потомке или используйте параметрический конструктор.");
            _serviceID = GenerateGuidFromString(ServiceName);

            IsSupportsCurrentStatusInfo = false;
        }

        public ServiceBase(Guid serviceID) : this(serviceID, string.Empty)
        {
        }

        public ServiceBase(Guid serviceID, string serviceName)
        {
            if (serviceID == null) throw new ArgumentNullException(nameof(serviceID));
            _serviceID = serviceID;
            _serviceName = serviceName;
        }

        #region CoreComponentBase
        /// <summary>
        /// Вызывается при запуске сервиса.
        /// </summary>
        protected override void OnStarting()
        {
        }

        /// <summary>
        /// Вызывается при остановке сервиса.
        /// </summary>
        protected override void OnStop()
        {
        }
        #endregion

        protected void SetServiceStatus(ServiceStatus status, string statusDetailed = null)
        {
            this.ServiceStatus = status;
            this.ServiceStatusDetailed = statusDetailed;

            AppCore.Get<Monitor>().RegisterServiceStateWithoutJournal(this, status, statusDetailed);
        }

        public static Guid GenerateGuidFromString(string source)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.Default.GetBytes(source));
                return new Guid(hash);
            }
        }

        public void RunService()
        {
            var storedUserContext = AppCore.GetUserContextManager().GetCurrentUserContext();
            var moduleRouting = AppCore.Get<Modules.Routing.ModuleRouting>();

            try
            {
                moduleRouting?.ClearCurrentThreadCache();
                AppCore.GetUserContextManager().SetCurrentUserContext(AppCore.GetUserContextManager().GetSystemUserContext());
                OnRunService();
            }
            finally
            {
                AppCore.GetUserContextManager().SetCurrentUserContext(storedUserContext);
                moduleRouting?.ClearCurrentThreadCache();
            }
        }

        protected abstract void OnRunService();

        #region Свойства
        public Guid ServiceID
        {
            get => _serviceID;
        }

        public virtual string ServiceName
        {
            get => _serviceName;
        }

        public bool IsSupportsCurrentStatusInfo
        {
            get;
            protected set;
        }

        public ServiceStatus ServiceStatus
        {
            get;
            private set;
        }

        public string ServiceStatusDetailed
        {
            get;
            private set;
        }
        #endregion
    }
}