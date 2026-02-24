using Ardalis.Specification;
using Mystira.Domain.Enums;
using Mystira.Domain.Models;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find accounts by their status.
/// Results are ordered by creation date descending (newest first).
/// </summary>
public sealed class AccountByStatusSpec : Specification<Account>
{
    public AccountByStatusSpec(AccountStatus status)
    {
        Query
            .Where(a => a.Status == status)
            .OrderByDescending(a => a.CreatedAt)
            .AsNoTracking();
    }
}
