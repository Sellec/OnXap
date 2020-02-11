using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // todo ������ �����������.
namespace OnXap.Core.Db
{
    [Table("UserHistory")]
    public class UserHistory : UserBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdUserHistory { get; set; }

        [Column("id")]
        public int IdUser { get; set; }

        public DateTime DateChangeHistory { get; set; }

        /// <summary>
        /// ��. <see cref="IdUser"/>.
        /// </summary>
        public override int ID => IdUser;

        /// <summary>
        /// ��. <see cref="UserBase.name"/>. 
        /// </summary>
        public override string Caption => !string.IsNullOrEmpty(name) ? name : !string.IsNullOrEmpty(email) ? email : IdUser.ToString();
    }
}
