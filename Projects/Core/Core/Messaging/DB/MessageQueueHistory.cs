namespace OnXap.Messaging.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // todo ������ �����������.
    [Table("MessageQueueHistory")]
    partial class MessageQueueHistory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int IdQueueHistory { get; set; }

        public int IdQueue{ get; set; }

        public DateTime DateEvent { get; set; }

        public string EventText { get; set; }

        public bool IsSuccess { get; set; }
    }
}
