using OnUtils.Data;

namespace OnXap.Modules.FileManager.DB
{
    class DataContext : Core.Db.CoreContext
    {
        public IRepository<File> File { get; set; }
        public IRepository<FileRemoveQueue> FileRemoveQueue { get; set; }
    }
}