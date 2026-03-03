namespace Mystira.Shared.CQRS;

/// <summary>
/// Marker interface for queries (read operations).
/// Queries should NOT modify state.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the query</typeparam>
/// <remarks>
/// Queries represent read-only operations that return data without modifying
/// system state. They follow the Command Query Responsibility Segregation (CQRS)
/// pattern where queries are separated from commands.
///
/// Used with Wolverine for message-based CQRS pattern. Wolverine discovers
/// handlers by convention, so this interface is optional but useful for:
/// - Documentation and self-documenting code
/// - Applying policies (like caching) to all queries
/// - IDE filtering and navigation
///
/// Queries should be:
/// - Idempotent (same input = same output)
/// - Side-effect free
/// - Cacheable when appropriate
/// </remarks>
/// <example>
/// <code>
/// // Simple query
/// public record GetUserByIdQuery(Guid UserId) : IQuery&lt;User?&gt;;
///
/// // Query with filtering
/// public record SearchUsersQuery(
///     string? SearchTerm,
///     int Page,
///     int PageSize) : IQuery&lt;PagedResult&lt;UserSummary&gt;&gt;;
///
/// // Handler (Wolverine discovers by convention)
/// public static class GetUserByIdHandler
/// {
///     public static async Task&lt;User?&gt; Handle(
///         GetUserByIdQuery query,
///         IUserRepository repository)
///     {
///         return await repository.GetByIdAsync(query.UserId);
///     }
/// }
/// </code>
/// </example>
public interface IQuery<out TResponse>
{
}
