using System;

namespace OnXap.Core.Modules
{
    using Users;

    /// <summary>
    /// Предоставляет варианты результата выполнения функции проверки прав доступа (см. <see cref="ModuleCore.CheckPermission(IUserContext, Guid)"/>).
    /// </summary>
    public enum CheckPermissionVariant
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

    /// <summary>
    /// Представляет результат выполнения функции проверки прав доступа (см. <see cref="ModuleCore.CheckPermission(IUserContext, Guid)"/>).
    /// </summary>
    public struct CheckPermissionResult
    {
        /// <summary>
        /// </summary>
        internal CheckPermissionResult(CheckPermissionVariant checkResult)
        {
            CheckResult = checkResult;
        }

        /// <summary>
        /// Возвращает true, если <see cref="CheckResult"/> равен <see cref="CheckPermissionVariant.Allowed"/>.
        /// </summary>
        public bool IsSuccess { get => CheckResult == CheckPermissionVariant.Allowed; }

        /// <summary>
        /// Дополнительная информация о результате проверки прав доступа.
        /// </summary>
        public CheckPermissionVariant CheckResult { get; }

        /// <summary>
        /// </summary>
        public static implicit operator bool(CheckPermissionResult checkPermissionResult)
        {
            return checkPermissionResult.IsSuccess;
        }

        /// <summary>
        /// </summary>
        public static implicit operator CheckPermissionVariant(CheckPermissionResult checkPermissionResult)
        {
            return checkPermissionResult.CheckResult;
        }
    }
}
