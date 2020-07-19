using System;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

#pragma warning disable
namespace OnXap.Core.Data.Helpers
{
    public class QueryInfo
    {
        private object _parametersRaw;
        private object _parametersSyncRoot = new object();
        private Dictionary<string, string> _parameters;

        internal QueryInfo(string queryText, object parametersRaw)
        {
            _parametersRaw = parametersRaw;
            QueryText = queryText;
            StartTime = DateTime.Now;
        }

        public Dictionary<string, string> GetParameters()
        {
            lock(_parametersSyncRoot)
            {
                if (_parameters != null) return _parameters;

                if (_parametersRaw is DbParameterCollection dbParameters)
                {
                    _parameters = dbParameters.OfType<DbParameter>().ToDictionary(x => x.ParameterName, x => $"{x.Value}");
                }
                else if (_parametersRaw != null)
                {
                    if (_parametersRaw is IDictionary dictionary)
                    {
                        var parameters = new Dictionary<string, string>();
                        foreach (DictionaryEntry entry in dictionary)
                        {
                            parameters[(string)entry.Key] = $"{entry.Value}";
                        }
                        _parameters = parameters;
                    }
                    else
                    {
                        var type = _parametersRaw.GetType();
                        _parameters = type.GetProperties().ToDictionary(x => x.Name, x => $"{x.GetValue(_parametersRaw, null)}");
                    }
                }
                else
                {
                    _parameters = new Dictionary<string, string>();
                }
                return _parameters;
            }
        }

        public string QueryText { get; }

        public DateTime StartTime { get; }
        public TimeSpan? ExecutionTime { get; internal set; }
        public List<QueryInfo> CurrentExecutingQueries { get; internal set; }
    }

    /// <summary>
    /// </summary>
    public static class QueryCounterExtensions
    {
        private static ConcurrentDictionary<object, QueryInfo> _commandsTiming = new ConcurrentDictionary<object, QueryInfo>();
        //private static ThreadLocal<List<QueryInfo>> _commandsTimingByThread = new ThreadLocal<List<QueryInfo>>(() => new List<QueryInfo>(), true);

        internal static void OnExecuting(object commandUniqueObject, string queryText, object parameters)
        {
            var queryInfo = new QueryInfo(queryText, parameters);
            queryInfo.CurrentExecutingQueries = GetCurrentExecutingQueries();
            _commandsTiming[commandUniqueObject] = queryInfo;
            //_commandsTimingByThread.Value.Add(queryInfo);
        }

        internal static void OnExecuted(object commandUniqueObject)
        {
            var endTime = DateTime.Now;
            if (_commandsTiming.TryRemove(commandUniqueObject, out var queryInfo))
            {
                //_commandsTimingByThread.Value.Remove(queryInfo);
                queryInfo.ExecutionTime = endTime - queryInfo.StartTime;
                queryInfo.GetParameters();
                if (OnQueryExecuted != null) try { OnQueryExecuted(queryInfo); } catch { }
            }
        }

        public static List<QueryInfo> GetCurrentExecutingQueries()
        {
            return _commandsTiming.Values.ToList();
        }

        public static event Action<QueryInfo> OnQueryExecuted;
    }
}