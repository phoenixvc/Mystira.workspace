using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository interface for ContentBundle entity with domain-specific queries
/// </summary>
public interface IContentBundleRepository : IRepository<ContentBundle>
{
    Task<IEnumerable<ContentBundle>> GetByAgeGroupAsync(string ageGroup);
}

