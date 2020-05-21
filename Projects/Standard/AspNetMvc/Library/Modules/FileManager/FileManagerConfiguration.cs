using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnXap.Modules.FileManager
{
    using Core.Configuration;

    public class FileManagerConfiguration : ModuleConfiguration<FileManager>
    {
        /// <summary>
        /// Возвращает или задает статус задачи проверки удаленных файлов.
        /// </summary>
        public bool IsCheckRemovedFiles
        {
            get => Get("IsCheckRemovedFiles", true);
            set => Set("IsCheckRemovedFiles", value);
        }
    }
}
