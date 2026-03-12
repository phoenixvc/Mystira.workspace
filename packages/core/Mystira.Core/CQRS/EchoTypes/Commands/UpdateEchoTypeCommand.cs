using Mystira.Domain.Models;

namespace Mystira.Core.CQRS.EchoTypes.Commands;

/// <summary>
/// Command to update an existing echo type.
/// </summary>
/// <param name="Id">The unique identifier of the echo type to update.</param>
/// <param name="Name">The new name of the echo type.</param>
/// <param name="Description">The new description of the echo type.</param>
public record UpdateEchoTypeCommand(string Id, string Name, string Description) : ICommand<EchoTypeDefinition?>;
