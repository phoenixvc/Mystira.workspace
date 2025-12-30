using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.EchoTypes.Commands;

/// <summary>
/// Command to update an existing echo type.
/// </summary>
public record UpdateEchoTypeCommand(string Id, string Name, string Description) : ICommand<EchoTypeDefinition?>;
