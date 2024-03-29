﻿using OnUtils.Architecture.ObjectPool;
using System;

namespace OnXap.Messaging.Components
{
    using Core;
    using Messages;

    /// <summary>
    /// Базовый класс компонента сервиса обработки сообщений определенного типа.
    /// </summary>
    public abstract class MessagingServiceComponent<TMessage> : 
        CoreComponentBase, 
        IPoolObjectOrdered, 
        IComponentTransient,
        IInternal
        where TMessage : MessageBase, new()
    {
        private readonly string _name;
        private readonly uint? _order;

        /// <summary>
        /// Создает новый экземпляр компонента.
        /// </summary>
        protected MessagingServiceComponent() : this(null, null)
        {
        }

        /// <summary>
        /// Создает новый экземпляр компонента.
        /// </summary>
        /// <param name="name">Имя компонента</param>
        /// <param name="usingOrder">Определяет очередность вызова компонента, если существует несколько компонентов, обрабатывающих один вид сообщений.</param>
        protected MessagingServiceComponent(string name, uint? usingOrder = null)
        {
            _name = !string.IsNullOrEmpty(name) ? name : GetType().FullName.GenerateGuid().ToString();
            _order = usingOrder;
        }

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStarting()
        {
        }

        /// <summary>
        /// Выполняется при запуске экземпляра компонента.
        /// </summary>
        protected abstract bool OnStartComponent();

        /// <summary>
        /// Выполняется при остановке экземпляра компонента.
        /// </summary>
        protected override void OnStop()
        {
        }
        #endregion

        #region Свойства
        /// <summary>
        /// Возвращает сериализованные настройки компонента, если они существуют.
        /// </summary>
        public string SerializedSettings { get; private set; }
        #endregion

        #region IInternal
        string IInternal.SerializedSettings
        {
            set => SerializedSettings = value;
        }

        bool IInternal.OnStartComponent()
        {
            return OnStartComponent();
        }
        #endregion

        #region IPoolObjectOrdered
        uint IPoolObjectOrdered.OrderInPool
        {
            get => _order ?? 0;
        }
        #endregion
    }

}
