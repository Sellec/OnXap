using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.Users.Db
{
    [Table("UserEntity")]
    class UserEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdEntity { get; set; }

        public int IdUser { get; set; }

        [Required]
        [StringLength(200)]
        public string Tag { get; set; }

        [Required]
        [StringLength(200)]
        public string EntityType { get; set; }

        [Required]
        public string Entity { get; set; }

        [Required]
        public bool IsTagged { get; set; }

        [MaxLength(200)]
        public string UniqueKey { get; set; }

    }
}
