using Mystira.App.Domain.Models;
using Mystira.Shared.CQRS;

namespace Mystira.App.Application.CQRS.Auth.Commands;

public record BootstrapAccountCommand(
    string ExternalUserId,
    string Email,
    string? DisplayName
) : ICommand<Account?>;
