namespace OnXap.Users
{
    /// <summary>
    /// Представляет результат выполнения операции проверки реквизитов или авторизации.
    /// </summary>
    public struct ExecutionAuthResult
    {
        /// <summary>
        /// </summary>
        public ExecutionAuthResult(eAuthResult authResult, string message = null)
        {
            IsSuccess = authResult == eAuthResult.Success;
            Message = message;
            AuthResult = authResult;
        }

        /// <summary>
        /// Признак успешности выполнения.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Текстовое сообщение об ошибке или об успехе. Может быть пустым.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Дополнительная информация о результате выполнения.
        /// </summary>
        public eAuthResult AuthResult { get; }
    }

}
