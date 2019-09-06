using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Modules.Customer
{
    using Core.Modules;
    using Model;
    using Register.Model;
    using Core.Items;

    class Startup : IConfigureBindings, IExecuteStart
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<ModuleCustomer, ModuleStandard>();
            bindingsCollection.SetTransient<IModuleController<ModuleCustomer>>(typeof(ModuleControllerCustomer), typeof(ModuleControllerAdminCustomer));
        }

        void IExecuteStart<OnXApplication>.ExecuteStart(OnXApplication core)
        {
            core.Get<ItemsManager>().RegisterModuleItemType<ProfileEdit, ModuleCustomer>();
            core.Get<ItemsManager>().RegisterModuleItemType<PreparedForRegister, ModuleCustomer>();
            core.Get<ItemsManager>().RegisterModuleItemType<Register, ModuleCustomer>();
        }
    }
}