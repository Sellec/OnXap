using System;

namespace OnXap.Core.Items
{
    using Core.Db;

    /// <summary>
    /// Описывает объект через его свойства.
    /// </summary>
    public class ItemKey
    {
        /// <summary>
        /// Создает новый экземпляр объекта.
        /// </summary>
        public ItemKey()
        {
        }

        /// <summary>
        /// Создает новый экземпляр объекта.
        /// </summary>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="key"/> равен null.</exception>
        public ItemKey(int idType, int idItem)
        {
            IdType = idType;
            IdItem = idItem;
        }

        /// <summary>
        /// Идентификатор типа объекта.
        /// </summary>
        /// <seealso cref="ItemType.IdItemType"/>.
        public int IdType { get; set; }

        /// <summary>
        /// Идентификатор объекта.
        /// </summary>
        public int IdItem { get; set; }

        /// <summary>
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is ItemKey itemKey) return IdType == itemKey.IdType && IdItem == itemKey.IdItem;
            return base.Equals(obj);
        }

        /// <summary>
        /// </summary>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// </summary>
        public override string ToString()
        {
            return $"IdType={IdType}, IdItem={IdItem}";
        }
    }
}
