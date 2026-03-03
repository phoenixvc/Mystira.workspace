using Ardalis.Specification;
using Mystira.Domain.Models;

namespace Mystira.Application.Specifications;

/// <summary>
/// Specification to find an account by email address (case-insensitive).
/// Returns a single result.
/// </summary>
public sealed class AccountByEmailSpec : SingleEntitySpecification<Account>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountByEmailSpec"/> class.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    public AccountByEmailSpec(string email)
    {
        Query.Where(a => a.Email.ToLower() == email.ToLower());
    }
}

/// <summary>
/// Specification to find an account by external user ID (Entra External ID).
/// Returns a single result.
/// </summary>
public sealed class AccountByExternalUserIdSpec : SingleEntitySpecification<Account>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountByExternalUserIdSpec"/> class.
    /// </summary>
    /// <param name="externalUserId">The external user ID to search for.</param>
    public AccountByExternalUserIdSpec(string externalUserId)
    {
        Query.Where(a => a.ExternalUserId == externalUserId);
    }
}

/// <summary>
/// Specification to find an account by ID.
/// Returns a single result.
/// </summary>
public sealed class AccountByIdSpec : SingleEntitySpecification<Account>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountByIdSpec"/> class.
    /// </summary>
    /// <param name="id">The account identifier.</param>
    public AccountByIdSpec(string id)
    {
        Query.Where(a => a.Id == id);
    }
}

/// <summary>
/// Specification to get all active accounts.
/// </summary>
/// <summary>
/// Specification to get active accounts (with active subscriptions).
/// </summary>
public sealed class ActiveAccountsSpec : BaseEntitySpecification<Account>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ActiveAccountsSpec"/> class.
    /// </summary>
    public ActiveAccountsSpec()
    {
        Query.Where(a => a.Subscription.IsActive)
             .OrderByDescending(a => a.CreatedAt);
    }
}

/// <summary>
/// Specification to search accounts by email pattern.
/// </summary>
public sealed class AccountsByEmailPatternSpec : BaseEntitySpecification<Account>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountsByEmailPatternSpec"/> class.
    /// </summary>
    /// <param name="emailPattern">The email pattern to search for.</param>
    public AccountsByEmailPatternSpec(string emailPattern)
    {
        Query.Where(a => a.Email.ToLower().Contains(emailPattern.ToLower()))
             .OrderBy(a => a.Email);
    }
}

/// <summary>
/// Specification for paginated account list with optional search.
/// </summary>
public sealed class AccountsPaginatedSpec : BaseEntitySpecification<Account>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountsPaginatedSpec"/> class.
    /// </summary>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The number of records to take.</param>
    /// <param name="searchTerm">Optional search term to filter by email.</param>
    public AccountsPaginatedSpec(int skip, int take, string? searchTerm = null)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            Query.Where(a => a.Email.ToLower().Contains(searchTerm.ToLower()));
        }

        Query.OrderBy(a => a.Email)
             .Skip(skip)
             .Take(take);
    }
}
