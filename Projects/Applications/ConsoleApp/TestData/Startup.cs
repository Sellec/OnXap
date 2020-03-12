using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;
using OnXap;

namespace ConsoleApp.TestData
{
    class Startup : IConfigureBindings
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<ModuleTest>();
        }
    }
}
