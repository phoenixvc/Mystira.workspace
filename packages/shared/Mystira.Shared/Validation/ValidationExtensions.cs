using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Mystira.Shared.Validation;

/// <summary>
/// Extension methods for registering FluentValidation with dependency injection
/// and configuring Wolverine validation middleware.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Registers FluentValidation validators from the assembly containing the specified marker type.
    /// </summary>
    /// <typeparam name="TAssemblyMarker">A type from the assembly containing validators</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddValidation<TAssemblyMarker>(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<TAssemblyMarker>();
        return services;
    }

    /// <summary>
    /// Registers FluentValidation validators from multiple assemblies.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblyMarkerTypes">Types from each assembly containing validators</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddValidation(
        this IServiceCollection services,
        params Type[] assemblyMarkerTypes)
    {
        foreach (var markerType in assemblyMarkerTypes)
        {
            services.AddValidatorsFromAssemblyContaining(markerType);
        }
        return services;
    }

    /// <summary>
    /// Configures Wolverine to use the validation middleware for IValidatable messages.
    /// Call this in your Wolverine configuration.
    /// </summary>
    /// <param name="options">The Wolverine options</param>
    /// <returns>The Wolverine options for chaining</returns>
    /// <example>
    /// <code>
    /// builder.Host.UseWolverine(opts =>
    /// {
    ///     opts.UseFluentValidation();
    /// });
    /// </code>
    /// </example>
    public static WolverineOptions UseFluentValidation(this WolverineOptions options)
    {
        options.Policies.ForMessagesOfType<IValidatable>()
            .AddMiddleware(typeof(ValidationMiddleware<>));

        return options;
    }

    /// <summary>
    /// Validates an object manually using the registered validators.
    /// Useful for validation outside of the message pipeline.
    /// </summary>
    /// <typeparam name="T">The type to validate</typeparam>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="instance">The instance to validate</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The validation result</returns>
    public static async Task<ValidationResult> ValidateAsync<T>(
        this IServiceProvider serviceProvider,
        T instance,
        CancellationToken ct = default) where T : class
    {
        var validators = serviceProvider.GetServices<IValidator<T>>().ToList();

        if (validators.Count == 0)
        {
            return ValidationResult.Success();
        }

        var context = new ValidationContext<T>(instance);
        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        return failures.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.FromFluentValidation(failures);
    }

    /// <summary>
    /// Validates an object and throws ValidationException if invalid.
    /// </summary>
    /// <typeparam name="T">The type to validate</typeparam>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="instance">The instance to validate</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="Exceptions.ValidationException">Thrown when validation fails</exception>
    public static async Task ValidateAndThrowAsync<T>(
        this IServiceProvider serviceProvider,
        T instance,
        CancellationToken ct = default) where T : class
    {
        var result = await serviceProvider.ValidateAsync(instance, ct);

        if (!result.IsValid)
        {
            throw new Exceptions.ValidationException(
                "One or more validation errors occurred.",
                result.ToDictionary());
        }
    }
}
