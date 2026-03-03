namespace Mystira.Shared.Validation;

/// <summary>
/// Marker interface for messages that should be automatically validated
/// by the validation middleware before handler execution.
/// </summary>
/// <remarks>
/// When a command or query implements this interface, the ValidationMiddleware
/// will automatically run all registered IValidator&lt;T&gt; validators before
/// the handler is executed. If validation fails, a ValidationException is thrown.
/// </remarks>
/// <example>
/// <code>
/// public record CreateUserCommand(string Name, string Email)
///     : ICommand&lt;User&gt;, IValidatable;
///
/// public class CreateUserCommandValidator : AbstractValidator&lt;CreateUserCommand&gt;
/// {
///     public CreateUserCommandValidator()
///     {
///         RuleFor(x => x.Name).NotEmpty();
///         RuleFor(x => x.Email).EmailAddress();
///     }
/// }
/// </code>
/// </example>
public interface IValidatable
{
}
