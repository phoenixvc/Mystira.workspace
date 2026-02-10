using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Ports.Data;

public interface IArchetypeRepository : IMasterDataRepository<ArchetypeDefinition>
{
    Task<ArchetypeDefinition?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name);
}
