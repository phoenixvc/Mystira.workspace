using MediatR;

namespace Mystira.Authoring.Abstractions.Commands;

/// <summary>
/// Marker interface for commands that return a response.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the command.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Marker interface for commands that do not return a response.
/// </summary>
public interface ICommand : IRequest
{
}
