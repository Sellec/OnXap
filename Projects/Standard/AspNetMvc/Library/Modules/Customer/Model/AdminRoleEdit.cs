using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.Modules.Customer.Model
{
    using Core.DB;

    [NotMapped]
    public class AdminRoleEdit : Role
    {
        public AdminRoleEdit()
        { }

        public AdminRoleEdit(Role role)
        {
            IdRole = role.IdRole;
            NameRole = role.NameRole;
            IdUserChange = role.IdUserChange;
            DateChange = role.DateChange;
            IdUserCreate = role.IdUserCreate;
            DateCreate = role.DateCreate;
            IsHidden = role.IsHidden;

            Permissions = new List<string>();
        }

        [Display(Name = "Разрешения модуля")]
        public List<string> Permissions { get; set; }
    }
}