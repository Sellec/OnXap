using System;

namespace OnXap.Core.Items
{
    /// <summary>
    /// Содержит информацию о ссылке на объект.
    /// </summary>
    public class ItemInfo
    {
        /// <summary>
        /// Описание объекта, для которого представлена ссылка.
        /// </summary>
        public ItemKey ItemKey { get; set; }

        /// <summary>
        /// Идентификатор ссылки на объект. Является полностью уникальным.
        /// </summary>
        public Guid LinkId { get; set; }
    }
}
