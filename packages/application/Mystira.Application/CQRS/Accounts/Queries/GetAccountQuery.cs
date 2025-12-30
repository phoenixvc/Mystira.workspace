using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Accounts.Queries;

public record GetAccountQuery(string AccountId) : IQuery<Account?>;
