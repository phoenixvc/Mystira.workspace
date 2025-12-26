namespace Mystira.Shared.Mapping;

/// <summary>
/// Common extension methods for object mapping.
/// These extensions work with Mapperly-generated mappers.
/// </summary>
public static class MapperExtensions
{
    /// <summary>
    /// Maps a nullable source to a nullable destination.
    /// Returns null if the source is null.
    /// </summary>
    public static TDestination? MapOrNull<TSource, TDestination>(
        this IMapper<TSource, TDestination> mapper,
        TSource? source)
        where TSource : class
        where TDestination : class
    {
        return source is null ? null : mapper.Map(source);
    }

    /// <summary>
    /// Maps a collection of items, filtering out nulls.
    /// </summary>
    public static IEnumerable<TDestination> MapMany<TSource, TDestination>(
        this IMapper<TSource, TDestination> mapper,
        IEnumerable<TSource?> sources)
        where TSource : class
    {
        foreach (var source in sources)
        {
            if (source is not null)
            {
                yield return mapper.Map(source);
            }
        }
    }

    /// <summary>
    /// Maps a collection to a list.
    /// </summary>
    public static List<TDestination> MapToList<TSource, TDestination>(
        this IMapper<TSource, TDestination> mapper,
        IEnumerable<TSource> sources)
    {
        return sources.Select(mapper.Map).ToList();
    }

    /// <summary>
    /// Maps a collection to an array.
    /// </summary>
    public static TDestination[] MapToArray<TSource, TDestination>(
        this IMapper<TSource, TDestination> mapper,
        IEnumerable<TSource> sources)
    {
        return sources.Select(mapper.Map).ToArray();
    }
}
