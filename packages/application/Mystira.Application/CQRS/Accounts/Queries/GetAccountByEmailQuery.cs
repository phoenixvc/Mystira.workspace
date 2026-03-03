using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Accounts.Queries;

/// <summary>
/// Query to retrieve an account by email address.
/// </summary>
/// <param name="Email">The email address of the account to retrieve.</param>
public record GetAccountByEmailQuery(string Email) : IQuery<Account?>;
