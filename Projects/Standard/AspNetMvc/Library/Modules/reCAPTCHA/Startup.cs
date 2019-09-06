using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;
using System.Web.Mvc;

namespace OnXap.Modules.reCAPTCHA
{
    using Core.Modules;

    class Startup : IExecuteStart, IConfigureBindings
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<ModuleReCaptcha>();
            bindingsCollection.SetTransient<IModuleController<ModuleReCaptcha>, ModuleReCaptchaController>();
        }

        void IExecuteStart<OnXApplication>.ExecuteStart(OnXApplication core)
        {
            ModelValidatorProviders.Providers.Insert(0, new ModelValidatorProvider(core));
        }
    }
}