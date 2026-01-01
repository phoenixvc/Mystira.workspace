using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

public interface ICompassAxisRepository
{
    Task<List<CompassAxisDefinition>> GetAllAsync();
    Task<CompassAxisDefinition?> GetByIdAsync(string id);
    Task<CompassAxisDefinition?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name);
    Task AddAsync(CompassAxisDefinition axis);
    Task UpdateAsync(CompassAxisDefinition axis);
    Task DeleteAsync(string id);
}
