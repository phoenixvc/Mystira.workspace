using FluentValidation;
using Wolverine;

namespace Mystira.Shared.Validation;

/// <summary>
/// Wolverine middleware that validates IValidatable messages before handler execution.
/// Throws ValidationException if validation fails.
/// </summary>
/// <remarks>
/// This middleware is automatically applied to messages that implement IValidatable.
/// It discovers all registered IValidator&lt;TMessage&gt; implementations and runs them
/// before the handler executes.
/// </remarks>
/// <typeparam name="TMessage">The message type to validate</typeparam>
public class ValidationMiddleware<TMessage> where TMessage : IValidatable
{
    /// <summary>
    /// Runs before the message handler, validating the message.
    /// </summary>
    /// <param name="message">The message to validate</param>
    /// <param name="validators">All registered validators for this message type</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Continue if valid, throws ValidationException if invalid</returns>
    public static async Task<HandlerContinuation> BeforeAsync(
        TMessage message,
        IEnumerable<IValidator<TMessage>> validators,
        CancellationToken ct)
    {
        var validatorList = validators.ToList();

        if (validatorList.Count == 0)
        {
            return HandlerContinuation.Continue;
        }

        var context = new ValidationContext<TMessage>(message);

        var validationTasks = validatorList.Select(v => v.ValidateAsync(context, ct));
        var results = await Task.WhenAll(validationTasks);

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
        {
            var errors = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).ToArray());

            throw new Mystira.Shared.Exceptions.ValidationException(errors);
        }

        return HandlerContinuation.Continue;
    }
}

/// <summary>
/// Non-generic validation middleware for use with Wolverine policies.
/// </summary>
public static class ValidationMiddleware
{
    /// <summary>
    /// Creates a validation middleware type for the specified message type.
    /// </summary>
    public static Type GetMiddlewareType(Type messageType)
    {
        return typeof(ValidationMiddleware<>).MakeGenericType(messageType);
    }
}
