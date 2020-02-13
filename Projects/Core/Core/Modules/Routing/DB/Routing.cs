using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // todo внести комментарии.
namespace OnXap.Modules.Routing.DB
{
    [Table("UrlTranslation")]
    public class Routing
    {
        [Key]
        [Column("IdTranslation")]
        public int IdRoute { get; set; }

        [Column("IdTranslationType")]
        public RoutingType IdRoutingType { get; set; }

        public int IdModule { get; set; }

        public int IdItem { get; set; }

        public int IdItemType { get; set; }

        [Required]
        [StringLength(200)]
        public string Action { get; set; }

        public string Arguments { get; set; }

        private string _UrlFull;
        [Required]
        [StringLength(255)]
        public string UrlFull
        {
            get => _UrlFull;
            set => _UrlFull = UriExtensions.MakeRelativeFromUrl(value);
        }

        public int DateChange { get; set; }

        public int IdUserChange { get; set; }

        public bool IsFixedLength { get; set; }

        public string UniqueKey { get; set; }

    }
}
