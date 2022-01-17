using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.Messaging.DB
{
    [Table("MessagingContactData")]
    partial class MessagingContactData
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int IdMessagingContactData { get; set; }

        public int IdMessagingContact { get; set; }

        public int IdMessagingServiceType { get; set; }

        public string Data { get; set; }

    }
}
