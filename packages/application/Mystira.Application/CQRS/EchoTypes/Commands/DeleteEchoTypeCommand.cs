namespace Mystira.Application.CQRS.EchoTypes.Commands;

/// <summary>
/// Command to delete an echo type.
/// </summary>
/// <param name="Id">The unique identifier of the echo type to delete.</param>
public record DeleteEchoTypeCommand(string Id) : ICommand<bool>;
