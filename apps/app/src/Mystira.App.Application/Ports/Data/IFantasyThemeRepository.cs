using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Ports.Data;

public interface IFantasyThemeRepository : IMasterDataRepository<FantasyThemeDefinition>
{
    Task<FantasyThemeDefinition?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
}
