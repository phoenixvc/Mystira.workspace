using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository for managing EchoTypeDefinition entities.
/// </summary>
public interface IEchoTypeRepository : IMasterDataRepository<EchoTypeDefinition>
{
    /// <summary>
    /// Gets an echo type definition by its name.
    /// </summary>
    Task<EchoTypeDefinition?> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Checks if an echo type definition exists with the specified name.
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
}
