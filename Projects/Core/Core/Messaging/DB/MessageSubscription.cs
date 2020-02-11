using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.Messaging.DB
{
    [Table("MessageSubscription")]
    class MessageSubscription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdSubscription { get; set; }

        [Required]
        [StringLength(200)]
        public string NameSubscription { get; set; }

        public bool IsHidden { get; set; }

        public bool IsEnabled { get; set; }

        [StringLength(200)]
        public string UniqueKey { get; set; }
    }
}
