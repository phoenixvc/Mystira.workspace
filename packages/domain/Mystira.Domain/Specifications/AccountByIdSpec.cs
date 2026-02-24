using Ardalis.Specification;
using Mystira.Domain.Models;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find an account by its unique identifier.
/// Returns a single result with no tracking for read-only access.
/// </summary>
public sealed class AccountByIdSpec : Specification<Account>, ISingleResultSpecification<Account>
{
    public AccountByIdSpec(string accountId)
    {
        Query
            .Where(a => a.Id == accountId)
            .AsNoTracking();
    }
}
