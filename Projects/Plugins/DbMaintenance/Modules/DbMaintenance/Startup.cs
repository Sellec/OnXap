using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Modules.DbMaintenance
{
    class Startup : IConfigureBindings, IConfigureBindingsLazy
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<DbMaintenanceModule>();
            bindingsCollection.SetTransient<Db.DefaultSchemeItem>();
        }

        void IConfigureBindingsLazy<OnXApplication>.ConfigureBindingsLazy(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<DbMaintenanceModule>();
            bindingsCollection.SetTransient<Db.DefaultSchemeItem>();
        }
    }
}
