using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mystira.Shared.Messaging;
using Wolverine;

namespace Mystira.Shared.Extensions;

/// <summary>
/// Extension methods for configuring Wolverine messaging.
/// </summary>
public static class MessagingExtensions
{
    /// <summary>
    /// Adds Wolverine messaging with default configuration.
    /// Reads settings from the "Messaging" configuration section.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="configureWolverine">Optional additional Wolverine configuration.</param>
    /// <returns>The host application builder for chaining.</returns>
    public static IHostApplicationBuilder AddMystiraMessaging(
        this IHostApplicationBuilder builder,
        Action<WolverineOptions>? configureWolverine = null)
    {
        var options = new MessagingOptions();
        builder.Configuration.GetSection(MessagingOptions.SectionName).Bind(options);

        builder.Services.Configure<MessagingOptions>(
            builder.Configuration.GetSection(MessagingOptions.SectionName));

        builder.Host.UseWolverine(wolverine =>
        {
            ConfigureWolverineDefaults(wolverine, options);
            configureWolverine?.Invoke(wolverine);
        });

        return builder;
    }

    /// <summary>
    /// Adds Wolverine messaging with explicit options.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="options">Messaging options.</param>
    /// <param name="configureWolverine">Optional additional Wolverine configuration.</param>
    /// <returns>The host application builder for chaining.</returns>
    public static IHostApplicationBuilder AddMystiraMessaging(
        this IHostApplicationBuilder builder,
        MessagingOptions options,
        Action<WolverineOptions>? configureWolverine = null)
    {
        builder.Services.Configure<MessagingOptions>(o =>
        {
            o.ServiceBusConnectionString = options.ServiceBusConnectionString;
            o.AutoProvision = options.AutoProvision;
            o.DurabilityMode = options.DurabilityMode;
            o.UseTransactionalOutbox = options.UseTransactionalOutbox;
            o.Enabled = options.Enabled;
            o.ServiceName = options.ServiceName;
            o.MaxRetries = options.MaxRetries;
            o.InitialRetryDelaySeconds = options.InitialRetryDelaySeconds;
        });

        builder.Host.UseWolverine(wolverine =>
        {
            ConfigureWolverineDefaults(wolverine, options);
            configureWolverine?.Invoke(wolverine);
        });

        return builder;
    }

    private static void ConfigureWolverineDefaults(WolverineOptions wolverine, MessagingOptions options)
    {
        // Configure durability mode
        wolverine.Durability.Mode = options.DurabilityMode switch
        {
            Messaging.DurabilityMode.Solo => Wolverine.DurabilityMode.Solo,
            Messaging.DurabilityMode.Balanced => Wolverine.DurabilityMode.Balanced,
            Messaging.DurabilityMode.MediatorOnly => Wolverine.DurabilityMode.MediatorOnly,
            Messaging.DurabilityMode.Serverless => Wolverine.DurabilityMode.Serverless,
            _ => Wolverine.DurabilityMode.Balanced
        };

        // Configure retry policy
        wolverine.Policies.OnException<Exception>()
            .RetryWithCooldown(
                TimeSpan.FromSeconds(options.InitialRetryDelaySeconds),
                TimeSpan.FromSeconds(options.InitialRetryDelaySeconds * 2),
                TimeSpan.FromSeconds(options.InitialRetryDelaySeconds * 4));

        // Configure Azure Service Bus if connection string is provided
        if (!string.IsNullOrEmpty(options.ServiceBusConnectionString))
        {
            wolverine.UseAzureServiceBus(options.ServiceBusConnectionString)
                .AutoProvision(options.AutoProvision);
        }

        // Apply local queue settings
        wolverine.LocalQueue("default")
            .Sequential();
    }
}
