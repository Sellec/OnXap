using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OnXap.Core.Data
{
    /// <summary>
    /// </summary>
    public static partial class EfQueryableExtensions
    {
        /// <summary>
        /// Метод расширения, позволяющий параметризировать запрос вида Contains(itemkey).
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TQuery"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="values"></param>
        /// <param name="keySelector"></param>
        /// <param name="throwIfParameterCountExceeded"></param>
        /// <returns></returns>
        public static IQueryable<TQuery> In<TKey, TQuery>(
             this IQueryable<TQuery> queryable,
             IEnumerable<TKey> values,
             Expression<Func<TQuery, TKey>> keySelector,
             bool throwIfParameterCountExceeded = false)
        {
            return queryable.Internal(values, keySelector, false, throwIfParameterCountExceeded);
        }

        /// <summary>
        /// Метод расширения, позволяющий параметризировать запрос вида Not Contains(itemkey).
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TQuery"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="values"></param>
        /// <param name="keySelector"></param>
        /// <param name="throwIfParameterCountExceeded"></param>
        /// <returns></returns>
        public static IQueryable<TQuery> NotIn<TKey, TQuery>(
             this IQueryable<TQuery> queryable,
             IEnumerable<TKey> values,
             Expression<Func<TQuery, TKey>> keySelector,
             bool throwIfParameterCountExceeded = false)
        {
            return queryable.Internal(values, keySelector, true, throwIfParameterCountExceeded);
        }

        private static IQueryable<TQuery> Internal<TKey, TQuery>(
             this IQueryable<TQuery> queryable,
             IEnumerable<TKey> values,
             Expression<Func<TQuery, TKey>> keySelector,
             bool exclude,
             bool throwIfParameterCountExceeded)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (!values.Any())
            {
                return exclude ? queryable : queryable.Take(0);
            }

            var distinctValueList = values.Distinct().ToList();
            var distinctValueListCount = distinctValueList.Count;
            var distinctValues = Bucketize(distinctValueList);

            if (distinctValues.Length > 2048)
            {
                if (throwIfParameterCountExceeded)
                {
                    throw new ArgumentException("Too many parameters for SQL Server, reduce the number of parameters", nameof(keySelector));
                }
                else
                {
                    distinctValueList = distinctValueList.Take(distinctValueListCount).ToList();
                    var method = distinctValueList.GetType().GetMethod("Contains");
                    Expression<Func<List<TKey>>> valueAsExpression = () => distinctValueList;
                    var call = Expression.Call(valueAsExpression.Body, method, keySelector.Body);
                    var lambda = Expression.Lambda<Func<TQuery, bool>>(call, keySelector.Parameters);

                    return queryable.Where(lambda);
                }
            }

            var predicates = distinctValues
                .Select(v =>
                {
                    // Create an expression that captures the variable so EF can turn this into a parameterized SQL query
                    Expression<Func<TKey>> valueAsExpression = () => v;
                    return exclude ? Expression.NotEqual(keySelector.Body, valueAsExpression.Body) : Expression.Equal(keySelector.Body, valueAsExpression.Body);
                })
                .ToList();

            while (predicates.Count > 1)
            {
                if (exclude)
                    predicates = PairWise(predicates).Select(p => Expression.AndAlso(p.Item1, p.Item2)).ToList();
                else
                    predicates = PairWise(predicates).Select(p => Expression.OrElse(p.Item1, p.Item2)).ToList();
            }

            var body = predicates.Single();

            var clause = Expression.Lambda<Func<TQuery, bool>>(body, keySelector.Parameters);

            return queryable.Where(clause);
        }

        /// <summary>
        /// Break a list of items tuples of pairs.
        /// </summary>
        private static IEnumerable<(T, T)> PairWise<T>(this IEnumerable<T> source)
        {
            var sourceEnumerator = source.GetEnumerator();
            while (sourceEnumerator.MoveNext())
            {
                var a = sourceEnumerator.Current;
                sourceEnumerator.MoveNext();
                var b = sourceEnumerator.Current;

                yield return (a, b);
            }
        }

        private static TKey[] Bucketize<TKey>(List<TKey> distinctValueList)
        {
            // Calculate bucket size as 1,2,4,8,16,32,64,...
            var bucket = 1;
            while (distinctValueList.Count > bucket)
            {
                bucket *= 2;
            }

            // Fill all slots.
            var lastValue = distinctValueList.Last();
            for (var index = distinctValueList.Count; index < bucket; index++)
            {
                distinctValueList.Add(lastValue);
            }

            var distinctValues = distinctValueList.ToArray();
            return distinctValues;
        }
    }
}
