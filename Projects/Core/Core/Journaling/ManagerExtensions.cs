using OnUtils;
using OnUtils.Architecture.AppCore;
using System;

namespace OnXap
{
    using Core;
    using Core.Items;
    using Journaling;
    using Journaling.DB;
    using ExecutionResultJournalName = ExecutionResult<Journaling.DB.JournalNameDAO>;
    using ExecutionResultJournalOptions = ExecutionResult<Journaling.JournalOptions>;

    /// <summary>
    /// Методы расширений для <see cref="JournalingManager"/>.
    /// </summary>
    public static class ManagerExtensions
    {
        internal static Type GetJournalType(Type type)
        {
            var typeGeneric = OnUtils.Types.TypeHelpers.ExtractGenericInterface(type, typeof(ITypedJournalComponent<>));
            if (typeGeneric != null) return typeGeneric.GetGenericArguments()[0];

            return type;
        }

        /// <summary>
        /// Возвращает журнал на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого необходимо получить журнал.</param>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalName"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        public static ExecutionResult<Journaling.Model.JournalInfo> GetJournal(this IComponentSingleton component)
        {
            return component.GetAppCore().Get<JournalingManager>().GetJournalTyped(component.GetType());
        }

        /// <summary>
        /// Устанавливает свойства журнала на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="journalOptions">Параметры журнала.</param>
        /// <returns>Возвращает объект <see cref="ExecutionResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        public static ExecutionResult SetJournalOptions(this IComponentSingleton component, JournalOptions journalOptions)
        {
            return component.GetAppCore().Get<JournalingManager>().SetJournalOptions(component.GetType(), journalOptions);
        }

        /// <summary>
        /// Возвращает свойства журнала на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalOptions"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        public static ExecutionResultJournalOptions GetJournalOptions(this IComponentSingleton component)
        {
            return component.GetAppCore().Get<JournalingManager>().GetJournalOptions(component.GetType());
        }

        /// <summary>
        /// Регистрирует новый журнал или обновляет старый на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется журнал.</param>
        /// <param name="nameJournal">См. <see cref="JournalNameDAO.Name"/>.</param>
        /// <param name="journalOptions">Дополнительные параметры журнала.</param>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalName"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="nameJournal"/> представляет пустую строку или null.</exception>
        public static ExecutionResult<Journaling.Model.JournalInfo> RegisterJournal(this IComponentSingleton component, string nameJournal, JournalOptions journalOptions = null)
        {
            return component.GetAppCore().Get<JournalingManager>().RegisterJournalTypedInternal(component.GetType(), nameJournal, journalOptions);
        }

        #region RegisterEvent
        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult<int?> RegisterEvent(this IComponentSingleton component, EventType eventType, string eventInfo, string eventInfoDetailed = null)
        {
            return ManagerExtensions.RegisterEvent(component, eventType, JournalingManager.EventCodeDefault, eventInfo, eventInfoDetailed);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventCode">См. <see cref="JournalDAO.EventCode"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult<int?> RegisterEvent(this IComponentSingleton component, EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed = null)
        {
            return ManagerExtensions.RegisterEvent(component, eventType, eventCode, eventInfo, eventInfoDetailed, null);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="exception">См. <see cref="JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult<int?> RegisterEvent(this IComponentSingleton component, EventType eventType, string eventInfo, string eventInfoDetailed = null, Exception exception = null)
        {
            return ManagerExtensions.RegisterEvent(component, eventType, JournalingManager.EventCodeDefault, eventInfo, eventInfoDetailed, exception);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventCode">См. <see cref="JournalDAO.EventCode"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="exception">См. <see cref="JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult<int?> RegisterEvent(this IComponentSingleton component, EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed = null, Exception exception = null)
        {
            return ManagerExtensions.RegisterEvent(component, eventType, eventCode, eventInfo, eventInfoDetailed, null, exception);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="JournalDAO.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult<int?> RegisterEvent(this IComponentSingleton component, EventType eventType, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            //return component.GetAppCore().Get<JournalingManager>().RegisterJournalTyped(type, nameJournal);
            return ManagerExtensions.RegisterEvent(component, eventType, JournalingManager.EventCodeDefault, eventInfo, eventInfoDetailed, eventTime, exception);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventCode">См. <see cref="JournalDAO.EventCode"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="JournalDAO.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult<int?> RegisterEvent(this IComponentSingleton component, EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            //return component.GetAppCore().Get<JournalingManager>().RegisterJournalTyped(type, nameJournal);
            return component.GetAppCore().Get<JournalingManager>().RegisterEvent(component.GetType(), eventType, eventCode, eventInfo, eventInfoDetailed, eventTime, exception);
        }
        #endregion

        #region RegisterEventForItem
        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="relatedItem">См. <see cref="JournalDAO.ItemLinkId"/>.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult RegisterEventForItem(this IComponentSingleton component, ItemBase relatedItem, EventType eventType, string eventInfo, string eventInfoDetailed = null)
        {
            return ManagerExtensions.RegisterEventForItem(component, relatedItem, eventType, 0, eventInfo, eventInfoDetailed, null);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="relatedItem">См. <see cref="JournalDAO.ItemLinkId"/>.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventCode">См. <see cref="JournalDAO.EventCode"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult RegisterEventForItem(this IComponentSingleton component, ItemBase relatedItem, EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed = null)
        {
            return ManagerExtensions.RegisterEventForItem(component, relatedItem, eventType, eventCode, eventInfo, eventInfoDetailed, null);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="itemKey">См. <see cref="JournalDAO.ItemLinkId"/>.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult RegisterEventForItem(this IComponentSingleton component, ItemKey itemKey, EventType eventType, string eventInfo, string eventInfoDetailed = null)
        {
            return ManagerExtensions.RegisterEventForItem(component, itemKey, eventType, 0, eventInfo, eventInfoDetailed, null);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="itemKey">См. <see cref="JournalDAO.ItemLinkId"/>.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventCode">См. <see cref="JournalDAO.EventCode"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult RegisterEventForItem(this IComponentSingleton component, ItemKey itemKey, EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed = null)
        {
            return ManagerExtensions.RegisterEventForItem(component, itemKey, eventType, eventCode, eventInfo, eventInfoDetailed, null);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="relatedItem">См. <see cref="JournalDAO.ItemLinkId"/>.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventCode">См. <see cref="JournalDAO.EventCode"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="exception">См. <see cref="JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult RegisterEventForItem(this IComponentSingleton component, ItemBase relatedItem, EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed = null, Exception exception = null)
        {
            return ManagerExtensions.RegisterEventForItem(component, relatedItem, eventType, eventCode, eventInfo, eventInfoDetailed, null, exception);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="itemKey">См. <see cref="JournalDAO.ItemLinkId"/>.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventCode">См. <see cref="JournalDAO.EventCode"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="exception">См. <see cref="JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult RegisterEventForItem(this IComponentSingleton component, ItemKey itemKey, EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed = null, Exception exception = null)
        {
            return ManagerExtensions.RegisterEventForItem(component, itemKey, eventType, eventCode, eventInfo, eventInfoDetailed, null, exception);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="relatedItem">См. <see cref="JournalDAO.ItemLinkId"/>.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventCode">См. <see cref="JournalDAO.EventCode"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="JournalDAO.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult RegisterEventForItem(this IComponentSingleton component, ItemBase relatedItem, EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            return component.GetAppCore().Get<JournalingManager>().RegisterEventForItem(component.GetType(), relatedItem, eventType, eventCode, eventInfo, eventInfoDetailed, eventTime, exception);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="itemKey">См. <see cref="JournalDAO.ItemLinkId"/>.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventCode">См. <see cref="JournalDAO.EventCode"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="JournalDAO.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult RegisterEventForItem(this IComponentSingleton component, ItemKey itemKey, EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            return component.GetAppCore().Get<JournalingManager>().RegisterEventForItem(component.GetType(), itemKey, eventType, eventCode, eventInfo, eventInfoDetailed, eventTime, exception);
        }
        #endregion
    }
}
