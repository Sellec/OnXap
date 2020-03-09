using Microsoft.EntityFrameworkCore;

namespace OnXap.Modules.Lexicon.Db
{
    class DataContext : Core.Db.CoreContext
    {
        public DbSet<WordCase> WordCase { get; set; }
    }
}
