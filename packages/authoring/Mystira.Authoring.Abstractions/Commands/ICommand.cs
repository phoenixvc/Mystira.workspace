namespace Mystira.Authoring.Abstractions.Commands;

/// <summary>
/// Marker interface for authoring commands that return a response.
/// Used with Wolverine for message-based CQRS pattern.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the command.</typeparam>
public interface ICommand<out TResponse> : ICommand
{
}

/// <summary>
/// Marker interface for authoring commands that do not return a response.
/// Used with Wolverine for message-based CQRS pattern.
/// </summary>
public interface ICommand
{
}
