namespace OnXap.Core.Items
{
    /// <summary>
    /// См. <see cref="ItemBase"/>
    /// </summary>
    public interface IItemBase
    {
        /// <summary>
        /// См. <see cref="ItemBase.IdBase"/>.
        /// </summary>
        int IdBase { get; }

        /// <summary>
        /// См. <see cref="ItemBase.CaptionBase"/>.
        /// </summary>
        string CaptionBase { get; }
    }
}
