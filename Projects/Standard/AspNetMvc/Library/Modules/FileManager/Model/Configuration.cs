using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OnXap.Modules.FileManager.Model
{
    public class Configuration : Core.Modules.Configuration.SaveModel
    {
        public void ApplyConfiguration(FileManagerConfiguration source)
        {
            IsCheckRemovedFiles = source.IsCheckRemovedFiles;
        }

        [Display(Name = "Возвращает или задает статус задачи проверки удаленных файлов.")]
        public bool IsCheckRemovedFiles { get; set; }
    }
}