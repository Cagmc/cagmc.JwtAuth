using System.Linq.Expressions;

namespace cagmc.JwtAuth.WebApi.Common.Extensions;

public static class QueriableExtensions
{
    public static IQueryable<T> OrderBy<T>(
        this IQueryable<T> source,
        string split,
        string defaultSort,
        string? sortFilter,
        int? pageIndex,
        int? pageSize)
    {
        var filterParts = string.IsNullOrEmpty(sortFilter)
            ? defaultSort.Split(split)
            : sortFilter.Split(split);

        var column = filterParts[0];
        var direction = filterParts[1];

        var orderedQuery = direction == "asc"
            ? OrderBy(source, column)
            : OrderByDescending(source, column);

        if (pageIndex.HasValue && pageSize.HasValue)
        {
            var orderedPaginatedQuery = orderedQuery
                .Skip((pageIndex.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
            
            return orderedPaginatedQuery;
        }
        
        return orderedQuery;
    }

    public static IOrderedQueryable<T> OrderBy<T>(
        this IQueryable<T> source,
        string property)
    {
        return ApplyOrder(source, property, "OrderBy");
    }

    public static IOrderedQueryable<T> OrderByDescending<T>(
        this IQueryable<T> source,
        string property)
    {
        return ApplyOrder(source, property, "OrderByDescending");
    }

    public static IOrderedQueryable<T> ThenBy<T>(
        this IOrderedQueryable<T> source,
        string property)
    {
        return ApplyOrder(source, property, "ThenBy");
    }

    public static IOrderedQueryable<T> ThenByDescending<T>(
        this IOrderedQueryable<T> source,
        string property)
    {
        return ApplyOrder(source, property, "ThenByDescending");
    }

    private static IOrderedQueryable<T> ApplyOrder<T>(
        IQueryable<T> source,
        string property,
        string methodName)
    {
        var props = property.Split('.');
        var type = typeof(T);
        var arg = Expression.Parameter(type, "x");
        Expression expr = arg;
        foreach (var prop in props)
        {
            // use reflection (not ComponentModel) to mirror LINQ
            var pi = type.GetProperty(prop)!;
            expr = Expression.Property(expr, pi);
            type = pi.PropertyType;
        }

        var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
        var lambda = Expression.Lambda(delegateType, expr, arg);

        var result = typeof(Queryable).GetMethods().Single(
                method => method.Name == methodName
                          && method.IsGenericMethodDefinition
                          && method.GetGenericArguments().Length == 2
                          && method.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), type)
            .Invoke(null, [source, lambda]);
        return (IOrderedQueryable<T>)result!;
    }
}