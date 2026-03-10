using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Ports.Data;

public interface IEchoTypeRepository : IMasterDataRepository<EchoTypeDefinition>
{
    Task<EchoTypeDefinition?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
}
