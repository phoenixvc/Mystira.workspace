namespace Mystira.App.Application.Ports.Data;

/// <summary>
/// Common CRUD interface for master data repositories.
/// All master data entities (AgeGroups, Archetypes, EchoTypes, FantasyThemes, CompassAxes)
/// share this common contract, enabling generic handler logic.
/// </summary>
public interface IMasterDataRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(string id);
}
