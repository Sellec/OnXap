using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Modules.Lexicon
{
    class Startup : IConfigureBindings, IConfigureBindingsLazy
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<LexiconManager>();
            bindingsCollection.SetTransient<Db.WordCaseSchemeItem>();
        }

        void IConfigureBindingsLazy<OnXApplication>.ConfigureBindingsLazy(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<LexiconManager>();
            bindingsCollection.SetTransient<Db.WordCaseSchemeItem>();
        }
    }
}
