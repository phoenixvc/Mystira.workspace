using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Accounts.Queries;

public record GetAccountByEmailQuery(string Email) : IQuery<Account?>;
