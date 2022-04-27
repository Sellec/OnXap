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

        public virtual IOrderedQueryable<SortedRow<TItem>> BuildSortedQuery<TItem>(IQueryable<TItem> queryBase) 
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Объект, содержащий строку данных и дополнительные сведения о сортировке.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class SortedRow<TItem>
    {
        /// <summary>
        /// Строка данных из запроса.
        /// </summary>
        public TItem SourceRow { get; set; }

        /// <summary>
        /// Дополнительный индекс строки в рамках сортировки. 
        /// Используется в случаях, когда невозможно сохранить привязку к типу <see cref="IOrderedQueryable{T}"/> при дальнейшей 
        /// обработке результата выполнения метода <see cref="ListViewOptions.BuildSortedQuery{TItem}(IQueryable{TItem})"/>.
        /// </summary>
        public int SortedIndex { get; set; }
    }

}