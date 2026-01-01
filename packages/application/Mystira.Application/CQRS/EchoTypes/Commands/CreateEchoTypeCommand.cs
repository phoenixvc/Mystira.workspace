using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.EchoTypes.Commands;

/// <summary>
/// Command to create a new echo type.
/// </summary>
/// <param name="Name">The name of the echo type.</param>
/// <param name="Description">The description of the echo type.</param>
public record CreateEchoTypeCommand(string Name, string Description) : ICommand<EchoTypeDefinition>;
