using System;
using System.Collections.Generic;
using System.Text;
using OnUtils.Data;

namespace OnXap.Languages.DB
{
    public class DataContext : Core.Db.CoreContextBase
    {
        public IRepository<Language> Language { get; set; }
    }
}
