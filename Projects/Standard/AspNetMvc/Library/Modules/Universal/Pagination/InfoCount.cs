using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnXap.Modules.Universal.Pagination
{
    /// <summary>
    /// Информация о постраничном выводе.
    /// </summary>
    public class InfoCount
    {
        /// <summary>
        /// Общее количество объектов.
        /// </summary>
        public int ItemsCount { get; set; }

        /// <summary>
        /// Начальная позиция выборки (начинается с 1, не с 0). Для корректного использования в запросах не забыть отнять единицу.
        /// </summary>
        public int ItemPositionStart { get; set; }

        public int ItemPositionEnd { get; set; }

        /// <summary>
        /// Количество объектов на странице. Теоретическое значение из настроек или из <see cref="ListViewOptions.ItemsPerPage"/> , согласно которому рассчитывается количество страниц.
        /// </summary>
        public int ItemsPerPageTheory { get; set; }

        /// <summary>
        /// Количество объектов на текущей странице. Это может быть либо <see cref="ItemsPerPageTheory"/>, либо остаток, если количество объектов на последней странице меньше <see cref="ItemsPerPageTheory"/>.
        /// </summary>
        public int ItemsPerPage { get; set; }
    }
}