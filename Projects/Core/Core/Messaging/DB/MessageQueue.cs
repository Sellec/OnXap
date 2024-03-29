namespace OnXap.Messaging.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // todo ������ �����������.
    [Table("MessageQueue")]
    partial class MessageQueue
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int IdQueue { get; set; }

        public int IdMessageType  { get; set; }

        /// <summary>
        /// ����������� ���������. True - ��������. False - ���������.
        /// </summary>
        public bool Direction { get; set; }

        public DateTime DateCreate { get; set; }

        public MessageStateType StateType { get; set; }

        public string State { get; set; }

        public int? IdTypeComponent { get; set; }

        /// <summary>
        /// ����, �� ������� �������� ��������� ���������.
        /// </summary>
        public DateTime DateDelayed { get; set; }

        public DateTime? DateChange { get; set; }

        public string MessageInfo { get; set; }
    }
}
