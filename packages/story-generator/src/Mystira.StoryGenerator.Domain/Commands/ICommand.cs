namespace Mystira.StoryGenerator.Domain.Commands;

/// <summary>
/// Marker interface for commands with a response.
/// Wolverine discovers handlers by convention (static Handle method).
/// </summary>
public interface ICommand<out TResponse>
{
}

/// <summary>
/// Marker interface for commands without a response.
/// </summary>
public interface ICommand
{
}
