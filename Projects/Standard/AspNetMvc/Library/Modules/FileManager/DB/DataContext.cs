using Microsoft.EntityFrameworkCore;

namespace OnXap.Modules.FileManager.Db
{
    class DataContext : Core.Db.CoreContext
    {
        public DbSet<File> File { get; set; }
        public DbSet<FileRemoveQueue> FileRemoveQueue { get; set; }
    }
}