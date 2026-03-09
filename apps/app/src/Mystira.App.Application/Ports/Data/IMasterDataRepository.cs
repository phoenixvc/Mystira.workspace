namespace Mystira.App.Application.Ports.Data;

/// <summary>
/// Common CRUD interface for master data repositories.
/// All master data entities (AgeGroups, Archetypes, EchoTypes, FantasyThemes, CompassAxes)
/// share this common contract, enabling generic handler logic.
/// </summary>
public interface IMasterDataRepository<T> where T : class
{
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    Task<T?> GetByIdAsync(string id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}
