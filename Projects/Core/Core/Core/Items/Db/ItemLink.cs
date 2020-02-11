using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.Core.Items.Db
{
    using Core.Db;

    /// <summary>
    /// ��������� ������ �� ������ � �������.
    /// </summary>
    [Table("ItemLink")]
    public class ItemLink
    {
        /// <summary>
        /// ������������� ���� �������.
        /// </summary>
        /// <seealso cref="ItemType.IdItemType"/>
        [Key]
        [Column(Order = 0)]
        public int ItemIdType { get; set; }

        /// <summary>
        /// ������������� �������.
        /// </summary>
        [Key]
        [Column(Order = 1)]
        public int ItemId { get; set; }

        /// <summary>
        /// �������������� ��������� ���� ��� ������������� �������.
        /// </summary>
        [Key]
        [Column(Order = 2)]
        [MaxLength(200)]
        [Required(AllowEmptyStrings = true)]
        public string ItemKey { get; set; } = "";

        /// <summary>
        /// ���������� ������������� ������.
        /// </summary>
        public Guid LinkId { get; set; }

        /// <summary>
        /// ���� �������� ������.
        /// </summary>
        public DateTime DateCreate { get; set; }
    }
}

