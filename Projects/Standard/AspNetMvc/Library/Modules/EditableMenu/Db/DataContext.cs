using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnXap.Modules.EditableMenu.Db
{
    using Core.Db;

    public class DataContext : CoreContextBase
    {
        public DbSet<Menu> Menu { get; set; }
    }
}