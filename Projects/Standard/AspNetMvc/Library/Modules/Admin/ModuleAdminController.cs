using System.Web.Mvc;

namespace OnXap.Modules.Admin
{
    using Core.Modules;

    public sealed class ModuleAdminController : ModuleControllerUser<ModuleAdmin>
    {
        [ModuleAction(null, ModulesConstants.PermissionManageString)]
        public override ActionResult Index()
        {
            return display("admin.cshtml");
        }
    }

}
