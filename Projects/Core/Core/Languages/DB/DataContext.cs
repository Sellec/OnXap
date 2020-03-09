using Microsoft.EntityFrameworkCore;

namespace OnXap.Languages.DB
{
    public class DataContext : Core.Db.CoreContextBase
    {
        public DbSet<Language> Language { get; set; }
    }
}
