using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.Languages.DB
{
    [Table("Language")]
    public class Language
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdLanguage { get; set; }

        [Required]
        [StringLength(200)]
        public string NameLanguage { get; set; }

        [Required]
        [StringLength(20)]
        public string ShortAlias { get; set; }

        public int IsDefault { get; set; }

        [Required]
        [StringLength(200)]
        public string TemplatesPath { get; set; }

        [Required]
        [StringLength(20)]
        public string Culture { get; set; }

    }
}
