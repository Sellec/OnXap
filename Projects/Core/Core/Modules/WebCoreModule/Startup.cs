using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Modules.WebCoreModule
{
    class Startup : IConfigureBindings
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.RegisterBindingConstraintHandler(new BindingConstraint());
            bindingsCollection.SetSingleton<WebCoreModule>();
            bindingsCollection.SetSingleton<WebCoreConfigurationChecker>();
        }
    }
}
