using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Ports.Data;

public interface ICompassAxisRepository : IMasterDataRepository<CompassAxis>
{
    Task<CompassAxis?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
}
