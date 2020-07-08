using Microsoft.EntityFrameworkCore;

namespace OnXap.TaskSheduling.Db
{
    class DataContext : Core.Db.CoreContextBase
    {
        public DbSet<Task> Task { get; set; }
        public DbSet<TaskSchedule> TaskSchedule { get; set; }
    }
}
