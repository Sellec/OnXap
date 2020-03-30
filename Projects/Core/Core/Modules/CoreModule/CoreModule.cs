using OnUtils.Architecture.AppCore;
using System;

namespace OnXap.Modules.CoreModule
{
    using Core.Modules;

    /// <summary>
    /// Интерфейс ядра системы для управления основными функциями.
    /// </summary>
    [ModuleCore("Ядро системы")]
    public sealed class CoreModule : ModuleCore<CoreModule>, ICritical
    {
        internal static readonly Guid PermissionConfigurationSave = "perm_configSave".GenerateGuid();

        /// <summary>
        /// </summary>
        protected override void OnModuleStarting()
        {
            RegisterPermission(PermissionConfigurationSave, "Изменение настроек сайта.", "");
        }
    }
}
