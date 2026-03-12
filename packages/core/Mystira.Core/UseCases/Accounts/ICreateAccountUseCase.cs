using Mystira.Core.CQRS.Accounts.Commands;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.UseCases.Accounts;

/// <summary>
/// Interface for CreateAccountUseCase to support dependency inversion
/// in Wolverine handlers and enable testability.
/// </summary>
public interface ICreateAccountUseCase
{
    Task<UseCaseResult<Account>> ExecuteAsync(CreateAccountCommand command, CancellationToken ct = default);
}
