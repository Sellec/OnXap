﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnXap.Modules.Universal.Pagination
{
    public class PagedView
    {
        /// <summary>
        /// текущая страница
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// общее количество страниц
        /// </summary>
        public int PageCount { get; set; }

        public Dictionary<int, int> stpg { get; set; }

        public Dictionary<int, int> fnpg { get; set; }

        public int np { get; set; }

        public bool PageFound { get; set; } = true;
    }
}