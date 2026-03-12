using Microsoft.Extensions.Logging;
using Mystira.Core.UseCases.Accounts;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.Accounts.Commands;

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
