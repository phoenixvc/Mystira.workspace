using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository interface for ContentBundle entity with domain-specific queries
/// </summary>
public interface IContentBundleRepository : IRepository<ContentBundle>
{
    /// <summary>
    /// Gets all content bundles for a specific age group.
    /// </summary>
    /// <param name="ageGroup">The age group identifier.</param>
    /// <returns>A collection of content bundles for the specified age group.</returns>
    Task<IEnumerable<ContentBundle>> GetByAgeGroupAsync(string ageGroup);
}

