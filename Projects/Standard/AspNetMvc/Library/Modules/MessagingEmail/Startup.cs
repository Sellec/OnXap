using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Modules.MessagingEmail
{
    using Core.Modules;

    class Startup : IConfigureBindings<OnXApplication>
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<EMailModule>();
            bindingsCollection.SetTransient<IModuleController<EMailModule>, EMailController>();
        }
    }
}