using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.Modules.Realty.DB
{
    using Core.MetadataObject;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    [Table("Realty")]
    public class Realty : MetadataObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public override int ID { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int IdAgency { get; set; }

        public string name { get; set; }
    }
}
