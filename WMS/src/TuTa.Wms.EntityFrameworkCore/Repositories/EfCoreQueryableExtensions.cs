using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Wms.EntityFrameworkCore
{
    public static class EfCoreQueryableExtensions
    {
        //public static IQueryable<T> IncludeIf<T, TResult>(
        //    this IQueryable<T> queryable,
        //    bool include, 
        //    Expression<Func<T, TResult>> predicate) where T : class
        //{
        //    return include ? queryable.Include(predicate) : queryable;
        //}

        public static IQueryable<T> TrackIf<T>(this DbSet<T> dbSet, bool isTrack) where T : class
        {
            return isTrack ? dbSet.AsQueryable() : dbSet.AsNoTracking();
        }
    }
}
