using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OnXap.Modules.Materials.Models
{
    /// <summary>
    /// Описывает модель данных, передаваемую для сохранения.
    /// </summary>
    public class NewsSave
    {
        /// <summary>
        /// Идентификатор материала.
        /// </summary>
        [DisplayName("Идентификатор материала")]
        public int IdMaterial { get; set; }

        [Required]
        [MaxLength(300)]
        [DisplayName("Название материала")]
        public string NameMaterial { get; set; }

        [DisplayName("Краткое содержимое")]
        public string BodyShort { get; set; }

        [DisplayName("Полное содержимое")]
        public string BodyFull { get; set; }

    }
}