using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // todo внести комментарии.
namespace OnXap.Core.Db
{
    using Core.Items;
    // todo вынести поля IdUserXX/DateXX из класса и писать события в журнал ролей.
    [Table("Role")]
    public class Role : ItemBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdRole { get; set; }

        [Display(Name = "Название роли")]
        [Required(ErrorMessage = "Название роли не может быть пустым")]
        [StringLength(200)]
        public string NameRole { get; set; }

        [Display(Name = "Скрытая роль")]
        public bool IsHidden { get; set; }

        [ScaffoldColumn(false)]
        public int IdUserCreate { get; set; }

        [ScaffoldColumn(false)]
        public int DateCreate { get; set; }

        [ScaffoldColumn(false)]
        public int IdUserChange { get; set; }

        [ScaffoldColumn(false)]
        public int DateChange { get; set; }

        [StringLength(100)]
        public string UniqueKey { get; set; }

        #region ItemBase
        public override int IdBase => IdRole;
        public override string CaptionBase => NameRole;
        #endregion
    }
}
