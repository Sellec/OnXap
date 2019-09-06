using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace OnXap.Modules.Customer.Model
{
    using Core.DB;
    using Journaling.Model;

    public class AdminUserEdit
    {
        public AdminUserEdit()
        {

        }

        [ScaffoldColumn(false)]
        public IList<JournalData> history;

        public User User { get; set; }

        [Display(Name = "Роли пользователя")]
        public IEnumerable<int> UserRoles { get; set; }

        public IEnumerable<SelectListItem> Roles;
    }
}