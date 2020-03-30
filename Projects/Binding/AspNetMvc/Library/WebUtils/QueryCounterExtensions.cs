using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#pragma warning disable
namespace OnXap.WebUtils
{
    /// <summary>
    /// </summary>
    public static class QueryCounterExtensions
    {
        public class QueryInfo
        {
            public string QueryText { get; set; }

            public Dictionary<string, string> Parameters { get; set; }

            public TimeSpan ExecutionTime { get; set; }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public static void AddQuery(string queryText, Dictionary<string, string> parameters, TimeSpan executionTime)
        {
            if (string.IsNullOrEmpty(queryText)) throw new ArgumentNullException(nameof(queryText));

            if (OnQueryExecuted != null) try { OnQueryExecuted(new QueryInfo() { QueryText = queryText, Parameters = parameters, ExecutionTime = executionTime }); } catch { }
        }

        public static event Action<QueryInfo> OnQueryExecuted;
    }
}