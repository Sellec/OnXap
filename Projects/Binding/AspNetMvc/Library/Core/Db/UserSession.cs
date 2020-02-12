using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.Core.Db
{
    [Table("UserSession")]
    class UserSession
    {
        [Key]
        [StringLength(24)]
        public string SessionId { get; set; }

        public DateTime Created { get; set; }

        public DateTime Expires { get; set; }

        public DateTime LockDate { get; set; }

        public int LockId { get; set; }

        public bool Locked { get; set; }

        public byte[] ItemContent { get; set; }

        public int IdUser { get; set; }

        public object SyncRoot = new object();

        public DateTime DateLastChanged = DateTime.Now;
        public DateTime DateLastSaved = DateTime.Now;
    }
}
