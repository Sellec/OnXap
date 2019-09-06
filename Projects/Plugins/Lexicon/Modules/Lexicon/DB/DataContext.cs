using OnUtils.Data;

namespace OnXap.Modules.Lexicon.DB
{
    class DataContext : Core.DB.CoreContext
    {
        public IRepository<WordCase> WordCase { get; set; }
    }
}
