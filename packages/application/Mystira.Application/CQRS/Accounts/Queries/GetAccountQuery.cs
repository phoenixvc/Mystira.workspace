using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Accounts.Queries;

/// <summary>
/// Query to retrieve an account by its unique identifier.
/// </summary>
/// <param name="AccountId">The unique identifier of the account to retrieve.</param>
public record GetAccountQuery(string AccountId) : IQuery<Account?>;
