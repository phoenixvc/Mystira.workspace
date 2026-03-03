using Microsoft.Extensions.Logging;
using Mystira.App.Application.UseCases.Accounts;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.CQRS.Accounts.Commands;

/// <summary>
/// Wolverine handler for CreateAccountCommand.
/// Delegates to CreateAccountUseCase for business logic.
/// </summary>
public static class CreateAccountCommandHandler
{
    public static async Task<Account?> Handle(
        CreateAccountCommand command,
        ICreateAccountUseCase useCase,
        ILogger logger,
        CancellationToken ct)
    {
        var result = await useCase.ExecuteAsync(command, ct);

        if (!result.IsSuccess)
        {
            logger.LogWarning("CreateAccount failed: {Error}", result.ErrorMessage);
            return null;
        }

        return result.Data;
    }
}
