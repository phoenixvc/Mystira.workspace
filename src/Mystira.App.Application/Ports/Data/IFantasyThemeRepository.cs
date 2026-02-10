using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Ports.Data;

public interface IFantasyThemeRepository : IMasterDataRepository<FantasyThemeDefinition>
{
    Task<FantasyThemeDefinition?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name);
}
