using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.Modules.Auth.Db
{
    [Table("UserPasswordRecovery")]
    class UserPasswordRecovery
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdRecovery { get; set; }

        public int IdUser { get; set; }

        [Required]
        [StringLength(32)]
        public string RecoveryKey { get; set; }
    }
}
