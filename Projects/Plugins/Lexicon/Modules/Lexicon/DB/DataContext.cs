using OnUtils.Data;

namespace OnXap.Modules.Lexicon.Db
{
    class DataContext : Core.Db.CoreContext
    {
        public IRepository<WordCase> WordCase { get; set; }
    }
}
