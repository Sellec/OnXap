﻿using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Modules.EditableMenu
{
    using Core.Modules;

    class Startup : IConfigureBindings
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<Module>();
            bindingsCollection.AddTransient<IModuleController<Module>, ModuleController>();
        }
    }
}