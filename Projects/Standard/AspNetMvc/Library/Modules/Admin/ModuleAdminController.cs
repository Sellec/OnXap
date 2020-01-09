using System.Web.Mvc;

namespace OnXap.Modules.Admin
{
    using Adminmain;
    using Core.Modules;

    public sealed class ModuleAdminController : ModuleControllerUser<ModuleAdmin>
    {
        [ModuleAction(null, ModulesConstants.PermissionManageString)]
        public override ActionResult Index()
        {
            return RedirectPermanent(Url.CreateRoute<Module, ModuleController>().ToString());
        }
    }

}
