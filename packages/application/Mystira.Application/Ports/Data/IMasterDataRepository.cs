namespace Mystira.Application.Ports.Data;

/// <summary>
/// Common CRUD interface for master data repositories.
/// All master data entities (AgeGroups, Archetypes, EchoTypes, FantasyThemes, CompassAxes)
/// share this common contract, enabling generic handler logic.
/// </summary>
public interface IMasterDataRepository<T> where T : class
{
    /// <summary>
    /// Gets all entities.
    /// </summary>
    Task<List<T>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    Task<T?> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    Task AddAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// Deletes an entity by its identifier.
    /// </summary>
    Task DeleteAsync(string id, CancellationToken ct = default);
}
