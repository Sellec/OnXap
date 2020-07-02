using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace OnXap.Core.Data.Helpers
{
    class QueryInterceptor : DbCommandInterceptor
    {
        private ConcurrentDictionary<DbCommand, DateTime> _commandsTiming = new ConcurrentDictionary<DbCommand, DateTime>();

        // NonQueryExecuting
        public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            QueryCounterExtensions.OnExecuting(command, command.CommandText, command.Parameters);
            return base.NonQueryExecuting(command, eventData, result);
        }

        public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
        {
            QueryCounterExtensions.OnExecuted(command);
            return base.NonQueryExecuted(command, eventData, result);
        }

        // NonQueryExecutingAsync
        public override Task<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default(CancellationToken))
        {
            QueryCounterExtensions.OnExecuting(command, command.CommandText, command.Parameters);
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override Task<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default(CancellationToken))
        {
            QueryCounterExtensions.OnExecuted(command);
            return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        // ReaderExecuting
        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            QueryCounterExtensions.OnExecuting(command, command.CommandText, command.Parameters);
            return base.ReaderExecuting(command, eventData, result);
        }

        public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
        {
            QueryCounterExtensions.OnExecuted(command);
            return base.ReaderExecuted(command, eventData, result);
        }

        // ReaderExecutingAsync
        public override Task<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default(CancellationToken))
        {
            QueryCounterExtensions.OnExecuting(command, command.CommandText, command.Parameters);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override Task<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default(CancellationToken))
        {
            QueryCounterExtensions.OnExecuted(command);
            return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        // ScalarExecuting
        public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
        {
            QueryCounterExtensions.OnExecuting(command, command.CommandText, command.Parameters);
            return base.ScalarExecuting(command, eventData, result);
        }

        public override object ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object result)
        {
            QueryCounterExtensions.OnExecuted(command);
            return base.ScalarExecuted(command, eventData, result);
        }

        // ScalarExecutingAsync
        public override Task<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default(CancellationToken))
        {
            QueryCounterExtensions.OnExecuting(command, command.CommandText, command.Parameters);
            return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override Task<object> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData, object result, CancellationToken cancellationToken = default(CancellationToken))
        {
            QueryCounterExtensions.OnExecuted(command);
            return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
        }
    }
}