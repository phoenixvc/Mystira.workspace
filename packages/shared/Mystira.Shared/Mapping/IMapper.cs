namespace Mystira.Shared.Mapping;

/// <summary>
/// Marker interface for Mapperly-generated mappers.
/// Implementations are source-generated at compile time.
/// </summary>
/// <typeparam name="TSource">The source type to map from</typeparam>
/// <typeparam name="TDestination">The destination type to map to</typeparam>
public interface IMapper<TSource, TDestination>
{
    /// <summary>
    /// Maps the source object to the destination type.
    /// </summary>
    /// <param name="source">The source object to map</param>
    /// <returns>A new instance of the destination type</returns>
    TDestination Map(TSource source);
}

/// <summary>
/// Bidirectional mapper interface for two-way conversions.
/// </summary>
/// <typeparam name="T1">The first type</typeparam>
/// <typeparam name="T2">The second type</typeparam>
public interface IBidirectionalMapper<T1, T2>
{
    /// <summary>
    /// Maps from T1 to T2.
    /// </summary>
    T2 MapForward(T1 source);

    /// <summary>
    /// Maps from T2 to T1.
    /// </summary>
    T1 MapReverse(T2 source);
}

/// <summary>
/// Collection mapper interface for mapping collections efficiently.
/// </summary>
/// <typeparam name="TSource">The source element type</typeparam>
/// <typeparam name="TDestination">The destination element type</typeparam>
public interface ICollectionMapper<TSource, TDestination> : IMapper<TSource, TDestination>
{
    /// <summary>
    /// Maps a collection of source objects to destination objects.
    /// </summary>
    IEnumerable<TDestination> MapAll(IEnumerable<TSource> sources);

    /// <summary>
    /// Maps a list of source objects to destination objects.
    /// </summary>
    IReadOnlyList<TDestination> MapList(IReadOnlyList<TSource> sources);
}
