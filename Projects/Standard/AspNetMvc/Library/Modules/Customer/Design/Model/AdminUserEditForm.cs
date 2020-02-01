using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace OnXap.Modules.Customer.Design.Model
{
    using Core.DB;
    using Journaling.Model;
    using Customer.Model;

    public class AdminUserEditForm : AdminUserEdit
    {
        [ScaffoldColumn(false)]
        public IList<JournalData> history;

        public IEnumerable<SelectListItem> Roles;
    }
}