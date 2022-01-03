namespace OnXap.Modules.Subscriptions
{
    /// <summary>
    /// Тип изменения, вносимого в список.
    /// </summary>
    public enum ChangeType
    {
        /// <summary>
        /// Добавить новых подписантов.
        /// </summary>
        Append,

        /// <summary>
        /// Установить новый список подписантов. Ранее подписанные, отсутствующие в переданном списке, будут отписаны.
        /// </summary>
        Replace,

        /// <summary>
        /// Удалить указанных подписантов.
        /// </summary>
        Remove
    }
}
