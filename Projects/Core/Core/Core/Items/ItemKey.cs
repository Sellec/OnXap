using System;

namespace OnXap.Core.Items
{
    /// <summary>
    /// Описывает объект через его свойства.
    /// </summary>
    public class ItemKey
    {
        private string _key = null;

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
        public ItemKey(int idType, int idItem, string key = "")
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            IdType = idType;
            IdItem = idItem;
            Key = key;
        }

        /// <summary>
        /// Идентификатор типа объекта.
        /// </summary>
        /// <seealso cref="DB.ItemType.IdItemType"/>.
        public int IdType { get; set; }

        /// <summary>
        /// Идентификатор объекта.
        /// </summary>
        public int IdItem { get; set; }

        /// <summary>
        /// Дополнительный ключ для идентификации объекта.
        /// </summary>
        public string Key
        {
            get => _key;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Key));
                if (!string.IsNullOrEmpty(value) && value.Length >= 200) throw new ArgumentOutOfRangeException(nameof(Key), "Длина значения не может быть больше 200 символов.");
                _key = value;
            }
        }

        /// <summary>
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is ItemKey itemKey) return IdType == itemKey.IdType && IdItem == itemKey.IdItem && Key == itemKey.Key;
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
            return $"IdType={IdType}, IdItem={IdItem}, Key={Key}";
        }
    }
}
