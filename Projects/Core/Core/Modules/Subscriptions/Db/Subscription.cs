using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.Modules.Subscriptions.Db
{
    [Table("Subscription")]
    class Subscription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdSubscription { get; set; }

        [Required]
        [StringLength(300)]
        public string NameSubscription { get; set; }

        public int IdGroup { get; set; }

        public Guid UniqueKey { get; set; }
    }
}
