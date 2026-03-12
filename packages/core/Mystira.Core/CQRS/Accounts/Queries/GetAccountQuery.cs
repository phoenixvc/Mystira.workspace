using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.Accounts.Queries;

public record GetAccountQuery(string AccountId) : IQuery<Account?>;
