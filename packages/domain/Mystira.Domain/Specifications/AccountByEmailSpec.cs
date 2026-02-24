using Ardalis.Specification;
using Mystira.Domain.Models;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find an account by email address (case-insensitive).
/// Uses NormalizedEmail for efficient, case-insensitive matching.
/// </summary>
public sealed class AccountByEmailSpec : Specification<Account>, ISingleResultSpecification<Account>
{
    public AccountByEmailSpec(string email)
    {
        Query
            .Where(a => a.NormalizedEmail == email.ToUpperInvariant());
    }
}
