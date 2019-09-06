using System;

namespace OnXap.Core.Modules
{
    using Users;

    /// <summary>
    /// Предоставляет варианты результата выполнения функции проверки прав доступа (см. <see cref="ModuleCore.CheckPermission(IUserContext, Guid)"/>).
    /// </summary>
    public enum CheckPermissionResult
    {
        /// <summary>
        /// Разрешено.
        /// </summary>
        Allowed,

        /// <summary>
        /// Запрещено.
        /// </summary>
        Denied,

        /// <summary>
        /// Разрешение не относится к модулю.
        /// </summary>
        PermissionNotFound,
    }
}
