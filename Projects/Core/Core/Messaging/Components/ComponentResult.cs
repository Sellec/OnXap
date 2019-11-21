using System;

namespace OnXap.Messaging.Components
{
    /// <summary>
    /// Представляет результат обработки сообщения в компоненте.
    /// </summary>
    public class ComponentResult
    {
        /// <summary>
        /// Указывает, что сообщение не было обработано. В таком случае сообщение передается следующему подходящему компоненту, либо попадет в обработку в следующем цикле.
        /// </summary>
        public const ComponentResult NotHandled = null;

        /// <summary>
        /// Указывает, что сообщение обработано и должно быть удалено из очереди обработки.
        /// </summary>
        public static readonly ComponentResult Complete = new ComponentResult() { StateType = Messages.MessageStateType.Completed };

        /// <summary>
        /// Указывает, что сообщение должно быть обработано повторно в следующих циклах в компоненте такого же типа. Это подходит для сообщений, которым требуется проверка состояния отправки во внешнем сервисе.
        /// </summary>
        /// <param name="dateDelayed">
        /// Дата следующей обработки сообщения. Если указано, то сообщение будет обработано только после наступления указанной даты.
        /// </param>
        /// <param name="state">
        /// Дополнительное состояние сообщения. 
        /// Позволяет переносить дополнительную информацию о состоянии сообщения.
        /// </param>
        /// <returns></returns>
        public static ComponentResult Repeat(DateTime? dateDelayed = null, string state = null)
        {
            if (!string.IsNullOrEmpty(state) && state.Length > 200) throw new ArgumentOutOfRangeException("Длина состояния не может превышать 200 символов.");
            return new ComponentResult() { StateType = Messages.MessageStateType.Repeat, State = state, DateDelayed = dateDelayed };
        }

        /// <summary>
        /// Указывает, что сообщение должно быть обработано позднее.
        /// </summary>
        /// <param name="dateDelayed">
        /// Дата следующей обработки сообщения.
        /// </param>
        /// <param name="state">
        /// Дополнительное состояние сообщения. 
        /// Позволяет переносить дополнительную информацию о состоянии сообщения.
        /// </param>
        /// <returns></returns>
        public static ComponentResult Delayed(DateTime dateDelayed, string state = null)
        {
            if (!string.IsNullOrEmpty(state) && state.Length > 200) throw new ArgumentOutOfRangeException("Длина состояния не может превышать 200 символов.");
            return new ComponentResult() { StateType = Messages.MessageStateType.Delayed, State = state, DateDelayed = dateDelayed };
        }

        /// <summary>
        /// Указывает, что сообщение было обработано с ошибкой и должно быть удалено из очереди обработки.
        /// </summary>
        /// <param name="error">Текст ошибки</param>
        /// <returns></returns>
        public static ComponentResult Error(string error)
        {
            if (!string.IsNullOrEmpty(error) && error.Length > 200) throw new ArgumentOutOfRangeException("Длина сообщения об ошибке не может превышать 200 символов.");
            return new ComponentResult() { StateType = Messages.MessageStateType.Error, State = error };
        }

        private ComponentResult()
        {
        }

        internal Messages.MessageStateType StateType { get; set; } = Messages.MessageStateType.NotHandled;

        internal string State { get; set; } = null;

        internal DateTime? DateDelayed { get; set; } = null;
    }
}
