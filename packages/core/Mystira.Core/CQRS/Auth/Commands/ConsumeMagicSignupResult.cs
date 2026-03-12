using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.Auth.Commands;

public record ConsumeMagicSignupResult(
    string Status,
    string Message,
    Account? Account
);
