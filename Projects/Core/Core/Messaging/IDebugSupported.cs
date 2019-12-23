namespace OnXap.Messaging
{
    /// <summary>
    /// Представляет методы управления режимом отладки для сервиса обработки сообщений.
    /// </summary>
    public interface IDebugSupported
    {
        /// <summary>
        /// Позволяет включать или отключать режим отладки для сервиса обработки сообщений.
        /// </summary>
        /// <param name="isEnable">Признак включения или отключения режима отладки.</param>
        /// <param name="debugRecipient">Адрес получателя сообщений для режима отладки.</param>
        /// <remarks>См. описание сервиса обработки сообщений, поддерживающего данный интерфейс, для просмотра ограничений в работе метода.</remarks>
        void SetDebugMode(bool isEnable, string debugRecipient);

        /// <summary>
        /// Возвращает состояние режима отладки для сервиса обработки сообщений.
        /// </summary>
        bool IsDebugModeEnabled();

        /// <summary>
        /// Возвращает отладочного получателя сообщений.
        /// </summary>
        /// <returns></returns>
        string GetDebugRecipient();
    }
}
