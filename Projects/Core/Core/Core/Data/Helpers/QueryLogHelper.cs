﻿using System;
using System.Collections.Generic;

namespace OnXap.Core.Data.Helpers
{
    /// <summary>
    /// </summary>
    public static class QueryLogHelper
    {
        static QueryLogHelper()
        {
            QueryCounterExtensions.OnQueryExecuted += (query) =>
            {
               if (QueryLogEnabled)
               {
                   if (_queries == null) _queries = new List<QueryInfo>();
                   _queries.Add(query);
               }
            };
        }

        /// <summary>
        /// Возвращает список запросов для текущего потока (см. <see cref="QueryLogEnabled"/> про потокозависимость).
        /// </summary>
        public static List<QueryInfo> GetQueries()
        {
            if (_queries == null) _queries = new List<QueryInfo>();
            return _queries;
        }

        /// <summary>
        /// Очищает историю запросов для текущего потока (см. <see cref="QueryLogEnabled"/> про потокозависимость).
        /// </summary>
        public static void ClearQueries()
        {
            if (_queries != null) _queries.Clear();
            _queries = null;
        }

        /// <summary>
        /// Позволяет включить или отключить механизм логирования запросов к БД.
        /// Параметр потокозависим - каждый поток логирует запросы независимо от остальных, соответственно метод <see cref="GetQueries"/> возвращает список запросов, выполненных в том потоке, в котором вызван метод.
        /// </summary>
        [ThreadStatic]
        public static bool QueryLogEnabled = default(bool);

        [ThreadStatic]
        private static List<QueryInfo> _queries;
    }
}
