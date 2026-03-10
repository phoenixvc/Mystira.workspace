using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.CQRS;

namespace Mystira.App.Application.CQRS.Auth.Commands;

public record BootstrapAccountCommand(
    string ExternalUserId,
    string Email,
    string? DisplayName
) : ICommand<Account?>;
