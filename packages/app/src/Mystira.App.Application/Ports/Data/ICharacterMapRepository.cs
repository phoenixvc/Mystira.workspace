using Mystira.App.Domain.Models;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Ports.Data;

/// <summary>
/// Repository interface for CharacterMap entity with domain-specific queries
/// </summary>
public interface ICharacterMapRepository : IRepository<CharacterMap, string>
{
    Task<CharacterMap?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
}

