using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace OnXap.Core.MetadataObject
{
    /// <summary>
    /// Методы расширения для получения свойств объектов метаданных.
    /// </summary>
    public static class IQueryableExtensions
    {
        /// <summary>
        /// Включает в запрос свойства объекта метаданных.
        /// </summary>
        public static IQueryable<TEntity> WithMetadataObjectProperties<TEntity>(this IQueryable<TEntity> source)
            where TEntity : MetadataObject
        {
            return source.Include(x => x.SourcePropertyData);
        }
    }
}
