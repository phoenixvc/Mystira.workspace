using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mystira.Shared.Caching;
using Mystira.Shared.Polyglot;
using Mystira.Shared.Messaging;
using Mystira.Shared.Resilience;

namespace Mystira.Shared.Extensions;

/// <summary>
/// Extension methods for adding options validation.
/// </summary>
public static class OptionsValidationExtensions
{
    /// <summary>
    /// Adds validated Mystira options to the service collection.
    /// Validates CacheOptions, ResilienceOptions, PolyglotOptions, and MessagingOptions.
    /// </summary>
    public static IServiceCollection AddMystiraOptionsValidation(this IServiceCollection services)
    {
        // Add validators
        services.AddSingleton<IValidateOptions<CacheOptions>, CacheOptionsValidator>();
        services.AddSingleton<IValidateOptions<ResilienceOptions>, ResilienceOptionsValidator>();
        services.AddSingleton<IValidateOptions<PolyglotOptions>, PolyglotOptionsValidator>();
        services.AddSingleton<IValidateOptions<MessagingOptions>, MessagingOptionsValidator>();

        return services;
    }
}

/// <summary>
/// Validator for CacheOptions.
/// </summary>
public class CacheOptionsValidator : IValidateOptions<CacheOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, CacheOptions options)
    {
        var errors = new List<string>();

        if (options.DefaultExpirationMinutes <= 0)
        {
            errors.Add($"{nameof(options.DefaultExpirationMinutes)} must be greater than 0. Current value: {options.DefaultExpirationMinutes}");
        }

        if (options.ShortExpirationMinutes <= 0)
        {
            errors.Add($"{nameof(options.ShortExpirationMinutes)} must be greater than 0. Current value: {options.ShortExpirationMinutes}");
        }

        if (options.LongExpirationMinutes <= 0)
        {
            errors.Add($"{nameof(options.LongExpirationMinutes)} must be greater than 0. Current value: {options.LongExpirationMinutes}");
        }

        if (options.ShortExpirationMinutes >= options.DefaultExpirationMinutes)
        {
            errors.Add($"{nameof(options.ShortExpirationMinutes)} ({options.ShortExpirationMinutes}) should be less than {nameof(options.DefaultExpirationMinutes)} ({options.DefaultExpirationMinutes})");
        }

        if (options.DefaultExpirationMinutes >= options.LongExpirationMinutes)
        {
            errors.Add($"{nameof(options.DefaultExpirationMinutes)} ({options.DefaultExpirationMinutes}) should be less than {nameof(options.LongExpirationMinutes)} ({options.LongExpirationMinutes})");
        }

        if (string.IsNullOrWhiteSpace(options.InstanceName))
        {
            errors.Add($"{nameof(options.InstanceName)} cannot be empty");
        }

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}

/// <summary>
/// Validator for ResilienceOptions.
/// </summary>
public class ResilienceOptionsValidator : IValidateOptions<ResilienceOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, ResilienceOptions options)
    {
        var errors = new List<string>();

        if (options.MaxRetries < 0)
        {
            errors.Add($"{nameof(options.MaxRetries)} cannot be negative. Current value: {options.MaxRetries}");
        }

        if (options.MaxRetries > 10)
        {
            errors.Add($"{nameof(options.MaxRetries)} should not exceed 10 to avoid excessive delays. Current value: {options.MaxRetries}");
        }

        if (options.BaseDelaySeconds <= 0)
        {
            errors.Add($"{nameof(options.BaseDelaySeconds)} must be greater than 0. Current value: {options.BaseDelaySeconds}");
        }

        if (options.CircuitBreakerThreshold <= 0)
        {
            errors.Add($"{nameof(options.CircuitBreakerThreshold)} must be greater than 0. Current value: {options.CircuitBreakerThreshold}");
        }

        if (options.CircuitBreakerDurationSeconds <= 0)
        {
            errors.Add($"{nameof(options.CircuitBreakerDurationSeconds)} must be greater than 0. Current value: {options.CircuitBreakerDurationSeconds}");
        }

        if (options.TimeoutSeconds <= 0)
        {
            errors.Add($"{nameof(options.TimeoutSeconds)} must be greater than 0. Current value: {options.TimeoutSeconds}");
        }

        if (options.LongRunningTimeoutSeconds <= options.TimeoutSeconds)
        {
            errors.Add($"{nameof(options.LongRunningTimeoutSeconds)} ({options.LongRunningTimeoutSeconds}) should be greater than {nameof(options.TimeoutSeconds)} ({options.TimeoutSeconds})");
        }

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}

/// <summary>
/// Validator for PolyglotOptions.
/// </summary>
public class PolyglotOptionsValidator : IValidateOptions<PolyglotOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, PolyglotOptions options)
    {
        var errors = new List<string>();

        if (options.CacheExpirationSeconds <= 0)
        {
            errors.Add($"{nameof(options.CacheExpirationSeconds)} must be greater than 0. Current value: {options.CacheExpirationSeconds}");
        }

        // Validate entity routing contains valid type names
        foreach (var (typeName, target) in options.EntityRouting)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                errors.Add("EntityRouting contains an empty type name key");
            }

            // Check if target is a valid enum value
            if (!Enum.IsDefined(typeof(DatabaseTarget), target))
            {
                errors.Add($"EntityRouting[{typeName}] has invalid DatabaseTarget value: {target}");
            }
        }

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}

/// <summary>
/// Validator for MessagingOptions.
/// </summary>
public class MessagingOptionsValidator : IValidateOptions<MessagingOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, MessagingOptions options)
    {
        var errors = new List<string>();

        if (options.MaxRetries < 0)
        {
            errors.Add($"{nameof(options.MaxRetries)} cannot be negative. Current value: {options.MaxRetries}");
        }

        if (options.MaxRetries > 10)
        {
            errors.Add($"{nameof(options.MaxRetries)} should not exceed 10. Current value: {options.MaxRetries}");
        }

        if (options.InitialRetryDelaySeconds <= 0)
        {
            errors.Add($"{nameof(options.InitialRetryDelaySeconds)} must be greater than 0. Current value: {options.InitialRetryDelaySeconds}");
        }

        // If Azure Service Bus is intended, connection string should be provided
        // Note: Empty is valid for local-only messaging

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
