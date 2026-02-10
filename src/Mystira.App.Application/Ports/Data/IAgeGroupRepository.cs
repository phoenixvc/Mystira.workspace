using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Ports.Data;

public interface IAgeGroupRepository : IMasterDataRepository<AgeGroupDefinition>
{
    Task<AgeGroupDefinition?> GetByNameAsync(string name);
    Task<AgeGroupDefinition?> GetByValueAsync(string value);
    Task<bool> ExistsByNameAsync(string name);
    Task<bool> ExistsByValueAsync(string value);
}
