using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // todo внести комментарии.
namespace OnXap.Modules.ItemsCustomize.DB
{
    [Table("CustomFieldsScheme")]
    public class CustomFieldsScheme
    {
        [Key]
        public int IdScheme { get; set; }

        public int IdModule { get; set; }

        [Required]
        [StringLength(200)]
        public string NameScheme { get; set; }

        [StringLength(500)]
        public string UniqueKey { get; set; }
    }
}
