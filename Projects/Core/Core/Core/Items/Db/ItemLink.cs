using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.Core.Items.Db
{
    using Core.Db;

    /// <summary>
    /// Описывает ссылку на объект в системе.
    /// </summary>
    [Table("ItemLink")]
    public class ItemLink
    {
        /// <summary>
        /// Идентификатор типа объекта.
        /// </summary>
        /// <seealso cref="ItemType.IdItemType"/>
        [Key]
        [Column(Order = 0)]
        public int ItemIdType { get; set; }

        /// <summary>
        /// Идентификатор объекта.
        /// </summary>
        [Key]
        [Column(Order = 1)]
        public int ItemId { get; set; }

        /// <summary>
        /// Дополнительный текстовый ключ для идентификации объекта.
        /// </summary>
        [Key]
        [Column(Order = 2)]
        [MaxLength(200)]
        [Required(AllowEmptyStrings = true)]
        public string ItemKey { get; set; } = "";

        /// <summary>
        /// Уникальный идентификатор ссылки.
        /// </summary>
        public Guid LinkId { get; set; }

        /// <summary>
        /// Дата создания ссылки.
        /// </summary>
        public DateTime DateCreate { get; set; }
    }
}

