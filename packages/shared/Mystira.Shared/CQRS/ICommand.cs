namespace Mystira.Shared.CQRS;

/// <summary>
/// Marker interface for commands (write operations) that don't return a result.
/// Commands modify state and should be idempotent when possible.
/// </summary>
/// <remarks>
/// Commands represent actions that change the system state. They follow the
/// Command Query Responsibility Segregation (CQRS) pattern where commands
/// are separated from queries.
///
/// Used with Wolverine for message-based CQRS pattern. Wolverine discovers
/// handlers by convention, so this interface is optional but useful for:
/// - Documentation and self-documenting code
/// - Applying policies (like validation) to all commands
/// - IDE filtering and navigation
/// </remarks>
/// <example>
/// <code>
/// // Command without result
/// public record DeactivateUserCommand(Guid UserId) : ICommand;
///
/// // Handler (Wolverine discovers by convention)
/// public static class DeactivateUserHandler
/// {
///     public static async Task Handle(
///         DeactivateUserCommand command,
///         IUserRepository repository)
///     {
///         var user = await repository.GetByIdAsync(command.UserId);
///         user?.Deactivate();
///         await repository.SaveChangesAsync();
///     }
/// }
/// </code>
/// </example>
public interface ICommand
{
}

/// <summary>
/// Marker interface for commands (write operations) that return a result.
/// Commands modify state and should be idempotent when possible.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the command</typeparam>
/// <remarks>
/// Use this for commands that need to return data after execution, such as:
/// - The created entity's ID
/// - A confirmation result
/// - The updated entity state
///
/// For commands that don't need to return data, use <see cref="ICommand"/> instead.
/// </remarks>
/// <example>
/// <code>
/// // Command with result
/// public record CreateUserCommand(string Name, string Email)
///     : ICommand&lt;User&gt;, IValidatable;
///
/// // Handler returns the created entity
/// public static class CreateUserHandler
/// {
///     public static async Task&lt;User&gt; Handle(
///         CreateUserCommand command,
///         IUserRepository repository)
///     {
///         var user = new User(command.Name, command.Email);
///         await repository.AddAsync(user);
///         return user;
///     }
/// }
/// </code>
/// </example>
public interface ICommand<out TResponse> : ICommand
{
}
