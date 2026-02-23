using Ardalis.Specification;
using Mystira.Domain.Enums;
using Mystira.Domain.Models;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to retrieve all active, non-deleted accounts with pagination.
/// Filters by Active status and excludes soft-deleted records.
/// Results are ordered by last login date descending.
/// </summary>
public sealed class ActiveAccountsSpec : Specification<Account>
{
    public ActiveAccountsSpec(int page = 1, int pageSize = 20)
    {
        Query
            .Where(a => a.Status == AccountStatus.Active && !a.IsDeleted)
            .OrderByDescending(a => a.LastLoginAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking();
    }
}
