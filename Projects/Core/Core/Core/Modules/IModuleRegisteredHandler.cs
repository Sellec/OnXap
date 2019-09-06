namespace OnXap.Core.Modules
{
    using Core;

    //todo описание
    public interface IModuleRegisteredHandler : IComponentSingleton
    {
        //todo описание
        void OnModuleInitialized<TModule>(TModule module) where TModule : ModuleCore<TModule>;
    }
}
