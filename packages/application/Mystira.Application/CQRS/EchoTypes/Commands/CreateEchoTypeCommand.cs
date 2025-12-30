using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.EchoTypes.Commands;

/// <summary>
/// Command to create a new echo type.
/// </summary>
public record CreateEchoTypeCommand(string Name, string Description) : ICommand<EchoTypeDefinition>;
