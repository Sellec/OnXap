using OnUtils.Data;

namespace OnXap.Modules.FileManager.DB
{
    class DataContext : Core.DB.CoreContext
    {
        public IRepository<File> File { get; set; }
        public IRepository<FileRemoveQueue> FileRemoveQueue { get; set; }
    }
}