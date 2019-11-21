﻿namespace OnXap.Messaging.Messages
{
    /// <summary>
    /// Варианты состояния сообщения <see cref="MessageInfo{TMessage}"/> после обработки в компоненте.
    /// </summary>
    public enum MessageStateType
    {
        /// <summary>
        /// Не обработано. Сообщение будет отправлено в следующий компонент или обработано в следующий раз, если других компонентов нет.
        /// </summary>
        NotHandled,

        /// <summary>
        /// Полностью обработанное сообщение, с которым больше не требуется предпринимать никаких действий.
        /// </summary>
        Completed,

        /// <summary>
        /// Ошибка отправки. 
        /// Такое сообщение больше не обрабатывается, считается отправленным. 
        /// Свойство <see cref="MessageInfo{TMessage}.State"/> может использоваться для хранения ошибки.
        /// </summary>
        Error,

        /// <summary>
        /// Сообщение помечено для обработки позднее.  обработаноОбработка сообщения пропущена Требуется повторная обработка в компоненте такого же типа. Это подходит для сообщений, которым требуется проверка состояния отправки во внешнем сервисе.
        /// </summary>
        /// <seealso cref="MessageInfo{TMessage}.State"/>
        Delayed,

        /// <summary>
        /// Требуется повторная обработка в компоненте такого же типа. Это подходит для сообщений, которым требуется проверка состояния отправки во внешнем сервисе.
        /// </summary>
        /// <seealso cref="MessageInfo{TMessage}.State"/>
        Repeat,
    }
}
