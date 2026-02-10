using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Ports.Data;

public interface ICompassAxisRepository : IMasterDataRepository<CompassAxis>
{
    Task<CompassAxis?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name);
}
