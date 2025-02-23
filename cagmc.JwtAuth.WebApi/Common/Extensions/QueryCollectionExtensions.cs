using System.Globalization;

namespace cagmc.JwtAuth.WebApi.Common.Extensions;

public static class QueryCollectionExtensions
{
    public static string? GetStringValue(this IQueryCollection query, string key)
    {
        return query.TryGetValue(key, out var values) ? values.FirstOrDefault() : null;
    }

    public static int? GetIntValue(this IQueryCollection query, string key)
    {
        return query.TryGetValue(key, out var values) ? int.Parse(values.FirstOrDefault()!) : null;
    }

    public static DateTime? GetDateTimeValue(this IQueryCollection query, string key)
    {
        return query.TryGetValue(key, out var values)
            ? DateTime.Parse(values.FirstOrDefault()!, new DateTimeFormatInfo())
            : null;
    }

    public static bool GetBoolValue(this IQueryCollection query, string key, bool defaultValue = false)
    {
        return query.TryGetValue(key, out var values) ? bool.Parse(values.FirstOrDefault()!) : defaultValue;
    }

    public static T? GetEnumValue<T>(this IQueryCollection query, string key) where T : struct
    {
        return query.TryGetValue(key, out var values) ? Enum.Parse<T>(values.FirstOrDefault()!) : null;
    }

    public static List<T>? GetEnumListValue<T>(this IQueryCollection query, string key) where T : struct
    {
        return query.TryGetValue(key, out var values) ? values.Select(x => Enum.Parse<T>(x!)).ToList() : null;
    }
}