using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnXap.Modules.Customer.Model
{
    public class AdminRolesManage
    {
        public IDictionary<int, AdminRoleEdit> Roles;
        public IEnumerable<SelectListItem> ModulesPermissions;
    }
}