using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Ports.Data;

public interface IEchoTypeRepository : IMasterDataRepository<EchoTypeDefinition>
{
    Task<EchoTypeDefinition?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
}
