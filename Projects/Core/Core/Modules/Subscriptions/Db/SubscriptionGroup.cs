using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.Modules.Subscriptions.Db
{
    [Table("SubscriptionGroup")]
    class SubscriptionGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdGroup { get; set; }

        [Required]
        [StringLength(300)]
        public string NameGroup { get; set; }

        public Guid UniqueKey { get; set; }
    }
}
