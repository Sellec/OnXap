namespace OnXap.Core.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // todo внести комментарии.
    [Table("ItemLink")]
    public partial class ItemLink
    {
        [Key]
        [Column(Order = 0)]
        public int ItemIdType { get; set; }

        [Key]
        [Column(Order = 1)]
        public int ItemId { get; set; }

        [Key]
        [Column(Order = 2)]
        [MaxLength(200)]
        [Required(AllowEmptyStrings = true)]
        public string ItemKey { get; set; } = "";

        public Guid LinkId { get; set; }

        public int? IdUser { get; set; }

        public DateTime DateCreate { get; set; }
    }
}

