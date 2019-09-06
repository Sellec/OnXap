namespace OnXap.Core.Items
{
    /// <summary>
    /// См. <see cref="ItemBase"/>
    /// </summary>
    public interface IItemBase
    {
        /// <summary>
        /// См. <see cref="ItemBase.ID"/>.
        /// </summary>
        int ID { get; }

        /// <summary>
        /// См. <see cref="ItemBase.Caption"/>.
        /// </summary>
        string Caption { get; }
    }
}
