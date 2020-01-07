using OnUtils.Data;

namespace OnXap.Modules.Adminmain
{
    using Core.Modules;

    /// <summary>
    /// Модуль управления основными функциями.
    /// </summary>
    [ModuleCore(" Система")]
    public class Module : ModuleCore<Module>, IUnitOfWorkAccessor<DataContext>
    {
        public const string PERM_CONFIGMAIN = "configuration_main";
        public const string PERM_MODULES = "modules";
        public const string PERM_RSS = "rss";
        public const string PERM_ADDRESS = "address_system";
        public const string PERM_ROUTING = "routing_system";
        public const string PERM_MANAGE_MESSAGING = "manage_messaging";
        public const string PERM_RESTART = "system_restart";

        protected override void OnModuleStart()
        {
            RegisterPermission(PERM_CONFIGMAIN, "Управление настройками");
            RegisterPermission(PERM_MODULES, "Управление модулями");
            RegisterPermission(PERM_RSS, "Управление фидом RSS");
            RegisterPermission(PERM_ADDRESS, "Система адресов (КЛАДР)");
            RegisterPermission(PERM_ROUTING, "Маршрутизация (ЧПУ)");
            RegisterPermission(PERM_MANAGE_MESSAGING, "Управление рассылками и уведомлениями");
            RegisterPermission(PERM_RESTART, "Перезагрузка веб-приложения");
        }
    }
}
