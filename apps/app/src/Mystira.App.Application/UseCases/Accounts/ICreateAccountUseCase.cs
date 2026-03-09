using Mystira.App.Application.CQRS.Accounts.Commands;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.UseCases.Accounts;

/// <summary>
/// Interface for CreateAccountUseCase to support dependency inversion
/// in Wolverine handlers and enable testability.
/// </summary>
public interface ICreateAccountUseCase
{
    Task<UseCaseResult<Account>> ExecuteAsync(CreateAccountCommand command, CancellationToken ct = default);
}
