using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.Messaging.DB
{
    [Table("MessagingContact")]
    partial class MessagingContact
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int IdMessagingContact { get; set; }

        public string NameFull { get; set; }

    }
}
