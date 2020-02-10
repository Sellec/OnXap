using OnUtils.Data;

namespace OnXap.Modules.Lexicon.DB
{
    class DataContext : Core.DBb.CoreContext
    {
        public IRepository<WordCase> WordCase { get; set; }
    }
}
