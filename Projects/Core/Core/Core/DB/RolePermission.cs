using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // todo ������ �����������.
namespace OnXap.Core.Db
{
    [Table("RolePermission")]
    public class RolePermission
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdRole { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdModule { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(200)]
        public string Permission { get; set; }

        public int IdUserChange { get; set; }

        public int DateChange { get; set; }

    }
}
