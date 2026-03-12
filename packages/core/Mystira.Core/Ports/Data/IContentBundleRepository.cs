using Mystira.Domain.Models;

namespace Mystira.Core.Ports.Data;

/// <summary>
/// Repository interface for ContentBundle entity with domain-specific queries
/// </summary>
public interface IContentBundleRepository : IRepository<ContentBundle>
{
    /// <summary>
    /// Gets all content bundles for a specific age group.
    /// </summary>
    Task<IEnumerable<ContentBundle>> GetByAgeGroupAsync(string ageGroup, CancellationToken ct = default);
}
