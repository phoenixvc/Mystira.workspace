using MediatR;

namespace Mystira.StoryGenerator.Domain.Commands;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

public interface ICommand : IRequest
{
}
