using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Ports.Data;

public interface IAgeGroupRepository : IMasterDataRepository<AgeGroupDefinition>
{
    Task<AgeGroupDefinition?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<AgeGroupDefinition?> GetByValueAsync(string value, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsByValueAsync(string value, CancellationToken ct = default);
}
