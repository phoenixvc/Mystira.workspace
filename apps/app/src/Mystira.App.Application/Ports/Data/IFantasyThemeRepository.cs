using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Ports.Data;

public interface IFantasyThemeRepository : IMasterDataRepository<FantasyThemeDefinition>
{
    Task<FantasyThemeDefinition?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
}
