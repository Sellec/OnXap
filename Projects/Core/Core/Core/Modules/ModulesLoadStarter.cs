using OnUtils.Architecture.AppCore;

namespace OnXap.Core.Modules
{
    using Core;

    class ModulesLoadStarter : CoreComponentBase, IComponentSingleton, IAutoStart
    {
        public ModulesLoadStarter()
        {
        }

        protected override void OnStart()
        {
            AppCore.GetModulesManager().StartModules();
        }

        protected override void OnStop()
        {
        }
    }
}
