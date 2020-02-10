using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace OnXap.Modules.Customer.Model
{
    using Core.Db;

    public class AdminUserEdit
    {
        [DefaultValue(false)]
        [Display(Name = "Сменить пароль")]
        public bool IsNeedToChangePassword { get; set; }

        public User User { get; set; }

        [Display(Name = "Роли пользователя")]
        public IEnumerable<int> UserRoles { get; set; }
    }
}