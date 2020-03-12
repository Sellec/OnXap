using System;

namespace OnXap.Modules.FileManager
{
    /// <summary>
    /// Перечисление, возвращаемое функцией <see cref="FileManager.Register(out Db.File, string, string, Guid?, DateTime?)"/>.
    /// </summary>
    public enum RegisterResult
    {
        /// <summary>
        /// </summary>
        Success = 0,

        /// <summary>
        /// </summary>
        Error = 1,

        /// <summary>
        /// Файл не был найден по указанному пути.
        /// </summary>
        NotFound = 2,
    }
}