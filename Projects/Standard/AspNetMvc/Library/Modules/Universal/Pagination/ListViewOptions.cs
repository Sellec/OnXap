using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnXap.Modules.Universal.Pagination
{
    public class ListViewOptions
    {
        public string Prefix { get; set; }

        public string sorting { get; set; }

        public int numpage { get; set; }

        //непонятный параметр, пока что закомментировал.
        //public int? skip { get; set; }

        public virtual IQueryable<SortedQuery<TItem>> BuildSortQuery<TItem>(IQueryable<TItem> query) 
        {
            return query.Select(x => new SortedQuery<TItem>() { Row = x, RowNumber = 0 });
        }
    }

    public class SortedQuery<TItem>
    {
        public TItem Row { get; set; }
        public long RowNumber { get; set; }
    }

}