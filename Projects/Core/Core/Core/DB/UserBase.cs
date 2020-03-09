using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#pragma warning disable CS1591 // todo внести комментарии.
namespace OnXap.Core.Db
{
    using Items;

    public abstract class UserBase : ItemBase
    {
        [DefaultValue("")]
        [StringLength(128)]
        [Display(Name = "Email-адрес"), DataType(DataType.EmailAddress), EmailAddress]
        public string email { get; set; }

        [DefaultValue("")]
        [StringLength(100)]
        [Display(Name = "Телефон"), DataType(DataType.PhoneNumber), PhoneFormat]
        public string phone { get; set; }

        [DefaultValue("")]
        [StringLength(100)]
        [Display(Name = "Имя"), Required]
        public string name { get; set; }

        [StringLength(1000)]
        [Display(Name = "О себе"), DataType(DataType.MultilineText)]
        public string about { get; set; }

        [Display(Name = "Фотография")]
        public int? IdPhoto { get; set; }

        [StringLength(64)]
        [Display(Name = "Пароль"), Required, DataType(DataType.Password)]
        public string password { get; set; }

        [StringLength(5)]
        public string salt { get; set; }

        public byte Superuser { get; set; }

        [Display(Name = "Состояние учетной записи"), Required]
        public UserState State { get; set; }

        [StringLength(100)]
        public string StateConfirmation { get; set; }

        public int AuthorizationAttempts { get; set; }

        public short Block { get; set; }

        public int BlockedUntil { get; set; }

        [StringLength(500)]
        public string BlockedReason { get; set; }

        public int DateChange { get; set; }

        [StringLength(200)]
        [Display(Name = "Уникальный ключ учетной записи")]
        public string UniqueKey { get; set; }

        public int IdUserChange { get; set; }

        [StringLength(2000)]
        [Display(Name = "Комментарий о себе"), DataType(DataType.MultilineText)]
        public string Comment { get; set; }

        [StringLength(2000)]
        [Display(Name = "Комментарий администратора"), DataType(DataType.MultilineText)]
        public string CommentAdmin { get; set; }
    }
}
