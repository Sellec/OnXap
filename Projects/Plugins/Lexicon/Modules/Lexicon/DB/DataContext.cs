using OnUtils.Data;

namespace OnXap.Modules.Lexicon.DB
{
    class DataContext : Core.Db.CoreContext
    {
        public IRepository<WordCase> WordCase { get; set; }
    }
}
