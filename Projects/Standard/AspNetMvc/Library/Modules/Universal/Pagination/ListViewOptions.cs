using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnXap.Modules.Universal.Pagination
{
    public class ListViewOptions
    {
        public virtual int ItemsPerPage { get; set; }

        //непонятный параметр, пока что закомментировал.
        //public int? skip { get; set; }

        public virtual IOrderedQueryable<SortedQuery<TItem>> BuildSortedQuery<TItem>(IQueryable<TItem> queryBase) 
        {
            throw new NotImplementedException();
        }
    }

    public class SortedQuery<TItem>
    {
        public TItem Row { get; set; }
    }

}