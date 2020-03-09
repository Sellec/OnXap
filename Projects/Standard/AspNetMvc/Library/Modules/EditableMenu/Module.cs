using OnUtils.Data;

namespace OnXap.Modules.EditableMenu
{
    using Core.Modules;

    /// <summary>
    /// Модуль управления редактируемыми меню.
    /// </summary>
    [ModuleCore("Редактируемые меню")]
    public class Module : ModuleCore<Module>
    {
        public const string PERM_EDITABLEMENU = "editablemenu";

        protected override void OnModuleStarting()
        {
            RegisterPermission(PERM_EDITABLEMENU, "Управление настраиваемыми меню");
        }

    }
}