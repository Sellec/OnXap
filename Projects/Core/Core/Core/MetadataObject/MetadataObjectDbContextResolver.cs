using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap.Core.MetadataObject
{
    class MetadataObjectDbContextResolver : IBindingsResolver<OnXApplication>
    {
        void IBindingsResolver<OnXApplication>.OnSingletonBindingResolve<TRequestedType>(ISingletonBindingsHandler<OnXApplication> bindingsHandler)
        {
            
        }

        void IBindingsResolver<OnXApplication>.OnTransientBindingResolve<TRequestedType>(ITransientBindingsHandler<OnXApplication> bindingsHandler)
        {
            var typeRequested = typeof(TRequestedType);
            if (typeof(Db.MetadataObjectDbContextBase).IsAssignableFrom(typeRequested) && !typeRequested.IsAbstract)
            {
                bindingsHandler.SetTransient<TRequestedType, TRequestedType>();
            }
        }
    }
}
