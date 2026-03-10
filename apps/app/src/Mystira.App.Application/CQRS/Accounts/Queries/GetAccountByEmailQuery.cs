using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.Accounts.Queries;

public record GetAccountByEmailQuery(string Email) : IQuery<Account?>;
