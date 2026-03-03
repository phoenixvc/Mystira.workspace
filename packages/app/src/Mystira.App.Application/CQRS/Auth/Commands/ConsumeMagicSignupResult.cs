using Mystira.App.Domain.Models;

namespace Mystira.App.Application.CQRS.Auth.Commands;

public record ConsumeMagicSignupResult(
    string Status,
    string Message,
    Account? Account
);
