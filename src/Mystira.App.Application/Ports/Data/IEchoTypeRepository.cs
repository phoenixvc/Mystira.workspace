using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Ports.Data;

public interface IEchoTypeRepository : IMasterDataRepository<EchoTypeDefinition>
{
    Task<EchoTypeDefinition?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name);
}
