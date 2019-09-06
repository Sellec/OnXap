using OnUtils.Architecture.AppCore;

namespace OnXap.Modules.Customer
{
    using Core.DB;

    class Startup : IExecuteStart
    {
        void IExecuteStart<OnXApplication>.ExecuteStart(OnXApplication core)
        {
            core.Get<Core.Items.ItemsManager>().RegisterModuleItemType<User, ModuleCustomer>();
        }
    }
}
