using OnUtils.Architecture.AppCore;

namespace OnXap
{
    using Core;

    /// <summary>
    /// Обеспечивает правильную очередность запуска компонентов ядра приложения. 
    /// По-умолчанию <see cref="AppCore{TAppCore}"/> сортирует <see cref="IAutoStart"/> сущности по алфавиту по полному имени типа, это не подходит, так как менеджер модулей должен запускаться первым.
    /// </summary>
    class ApplicationLauncher : CoreComponentBase, IComponentSingleton, ICritical
    {
        protected override void OnStarting()
        {
            var dbSchemaManager = AppCore.Get<Core.DbSchema.DbSchemaManager>();
            var modulesManager = AppCore.GetModulesManager();
        }

        protected override void OnStop()
        {
        }
    }
}
