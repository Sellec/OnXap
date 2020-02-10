using FluentMigrator;
using OnUtils.Architecture.AppCore;

namespace OnXap.Core.DbSchema
{
    /// <summary>
    /// Общий класс для работы со схемой базы данных.
    /// </summary>
    /// <seealso cref="Migration"/>
    /// <seealso cref="MigrationAttribute"/>
    /// <seealso cref="ProfileAttribute"/>
    public abstract class DbSchemaItem : Migration, IComponentTransient
    {
        class ComponentTransientImpl : CoreComponentBase, IComponentTransient
        {
            private readonly DbSchemaItem _instance;

            public ComponentTransientImpl(DbSchemaItem instance)
            {
                _instance = instance;
            }

            protected override void OnStart()
            {
            }

            protected override void OnStop()
            {
            }
        }

        private readonly ComponentTransientImpl _componentImpl;

        /// <summary>
        /// Создает новый экземпляр объекта.
        /// </summary>
        protected DbSchemaItem()
        {
            _componentImpl = new ComponentTransientImpl(this);
        }

        #region IComponentTransient
        /// <summary>
        /// </summary>
        public void Stop()
        {
            ((IComponentTransient)_componentImpl).Stop();
        }

        /// <summary>
        /// </summary>
        public CoreComponentState GetState()
        {
            return _componentImpl.GetState();
        }

        /// <summary>
        /// </summary>
        public OnXApplication GetAppCore()
        {
            return _componentImpl.GetAppCore();
        }

        /// <summary>
        /// </summary>
        public void Start(OnXApplication appCore)
        {
            ((IComponentTransient)_componentImpl).Start(appCore);
        }
        #endregion
    }
}
