using System;
using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap
{
    /// <summary>
    /// Позволяет осуществлять привязку типов.
    /// </summary>
    public sealed class BindingsCollection : IBindingsCollection<OnXApplication>
    {
        private IBindingsCollection<OnXApplication> _source;

        internal BindingsCollection(IBindingsCollection<OnXApplication> source)
        {
            _source = source;
        }

        public void RegisterBindingConstraintHandler(IBindingConstraintHandler handler)
        {
            _source.RegisterBindingConstraintHandler(handler);
        }

        #region ISingletonBindingsHandler
        public void SetSingleton<TQuery, TImplementation>()
            where TQuery : IComponentSingleton<OnXApplication>
            where TImplementation : TQuery
        {
            _source.SetSingleton<TQuery, TImplementation>();
        }

        public void SetSingleton<TQuery, TImplementation>(Func<TImplementation> factoryLambda)
            where TQuery : IComponentSingleton<OnXApplication>
            where TImplementation : TQuery
        {
            _source.SetSingleton<TQuery, TImplementation>(factoryLambda);
        }

        public void SetSingleton(Type queryType, Type implementationType)
        {
            _source.SetSingleton(queryType, implementationType);
        }

        public void SetSingleton<TQuery>()
            where TQuery : class, IComponentSingleton<OnXApplication>
        {
            _source.SetSingleton<TQuery>();
        }

        public void SetSingleton<TQuery>(Func<TQuery> factoryLambda)
            where TQuery : class, IComponentSingleton<OnXApplication>
        {
            _source.SetSingleton(factoryLambda);
        }
        #endregion

        #region ITransientBindingsHandler

        public void SetTransient<TQuery, TImplementation>()
             where TQuery : IComponentTransient<OnXApplication>
             where TImplementation : TQuery
        {
            _source.SetTransient<TQuery, TImplementation>();
        }

        public void SetTransient<TQuery>(params Type[] implementationTypes) 
            where TQuery : IComponentTransient<OnXApplication>
        {
            _source.SetTransient<TQuery>(implementationTypes);
        }


        public void SetTransient<TQuery>()
            where TQuery : class, IComponentTransient<OnXApplication>
        {
            _source.SetTransient<TQuery>();
        }

        public void AddTransient<TQuery, TImplementation>()
            where TQuery : IComponentTransient<OnXApplication>
            where TImplementation : TQuery, new()
        {
            _source.AddTransient<TQuery, TImplementation>();
        }
        #endregion
    }

    /// <summary>
    /// Базовый класс для регистрации привязок типов и выполнения операций во время запуска приложения.
    /// </summary>
    public abstract class StartupBase : IConfigureBindings, IConfigureBindingsLazy, IExecuteStart, IExecuteStartLazy
    {
        void IConfigureBindings<OnXApplication>.ConfigureBindings(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            ConfigureBindings(new BindingsCollection(bindingsCollection));
        }

        void IConfigureBindingsLazy<OnXApplication>.ConfigureBindingsLazy(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            ConfigureBindingsLazy(new BindingsCollection(bindingsCollection));
        }

        void IExecuteStart<OnXApplication>.ExecuteStart(OnXApplication core)
        {
            ExecuteStart(core);
        }

        void IExecuteStartLazy<OnXApplication>.ExecuteStartLazy(OnXApplication core)
        {
            ExecuteStartLazy(core);
        }

        /// <summary>
        /// Вызывается для определения привязок типов при запуске ядра.
        /// </summary>
        protected virtual void ConfigureBindings(BindingsCollection bindingsCollection)
        {
        }

        /// <summary>
        /// Вызывается для определения дополнительных привязок типов при загрузке сборок (<see cref="System.Reflection.Assembly"/>) после запуска ядра.
        /// </summary>
        protected virtual void ConfigureBindingsLazy(BindingsCollection bindingsCollection)
        {
        }

        /// <summary>
        /// Вызывается после запуска компонентов ядра.
        /// </summary>
        protected virtual void ExecuteStart(OnXApplication core)
        {
        }

        /// <summary>
        /// Вызывается при загрузке сборок (<see cref="System.Reflection.Assembly"/>) после запуска ядра.
        /// </summary>
        protected virtual void ExecuteStartLazy(OnXApplication core)
        {
        }
    }
}
