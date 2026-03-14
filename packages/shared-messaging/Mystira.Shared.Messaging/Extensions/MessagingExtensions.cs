using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mystira.Shared.Messaging;
using Wolverine;
using Wolverine.AzureServiceBus;
using Wolverine.ErrorHandling;

namespace Mystira.Shared.Extensions;

/// <summary>
/// Extension methods for configuring Wolverine messaging with Azure Service Bus.
/// </summary>
public static class MessagingExtensions
{
    /// <summary>
    /// Adds Wolverine messaging with default configuration.
    /// Reads settings from the "Messaging" configuration section.
    /// </summary>
    public static HostApplicationBuilder AddMystiraMessaging(
        this HostApplicationBuilder builder,
        Action<WolverineOptions>? configureWolverine = null)
    {
        var options = new MessagingOptions();
        builder.Configuration.GetSection(MessagingOptions.SectionName).Bind(options);

        builder.Services.Configure<MessagingOptions>(
            builder.Configuration.GetSection(MessagingOptions.SectionName));

        builder.UseWolverine(wolverine =>
        {
            ConfigureWolverineDefaults(wolverine, options);
            configureWolverine?.Invoke(wolverine);
        });

        return builder;
    }

    /// <summary>
    /// Adds Wolverine messaging with explicit options.
    /// </summary>
    public static HostApplicationBuilder AddMystiraMessaging(
        this HostApplicationBuilder builder,
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

        builder.UseWolverine(wolverine =>
        {
            ConfigureWolverineDefaults(wolverine, options);
            configureWolverine?.Invoke(wolverine);
        });

        return builder;
    }

    /// <summary>
    /// Configures Wolverine with Azure Service Bus including topics, subscriptions, and dead-letter queues.
    /// </summary>
    public static WolverineOptions UseAzureServiceBusWithTopics(
        this WolverineOptions wolverine,
        string connectionString,
        string serviceName)
    {
        wolverine.UseAzureServiceBus(connectionString)
            .AutoProvision();

        ConfigureErrorHandling(wolverine);

        return wolverine;
    }

    private static void ConfigureErrorHandling(WolverineOptions wolverine)
    {
        wolverine.OnException<Exception>()
            .RetryWithCooldown(
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30),
                TimeSpan.FromSeconds(60))
            .Then.MoveToErrorQueue();
    }

    private static void ConfigureWolverineDefaults(WolverineOptions wolverine, MessagingOptions options)
    {
        wolverine.Durability.Mode = options.DurabilityMode switch
        {
            Messaging.DurabilityMode.Solo => Wolverine.DurabilityMode.Solo,
            Messaging.DurabilityMode.Balanced => Wolverine.DurabilityMode.Balanced,
            Messaging.DurabilityMode.MediatorOnly => Wolverine.DurabilityMode.MediatorOnly,
            Messaging.DurabilityMode.Serverless => Wolverine.DurabilityMode.Serverless,
            _ => Wolverine.DurabilityMode.Balanced
        };

        var retryDelays = GenerateRetryDelays(options.MaxRetries, options.InitialRetryDelaySeconds);
        if (retryDelays.Length > 0)
        {
            wolverine.OnException<Exception>()
                .RetryWithCooldown(retryDelays);
        }

        if (!string.IsNullOrEmpty(options.ServiceBusConnectionString))
        {
            wolverine.UseAzureServiceBusWithTopics(
                options.ServiceBusConnectionString,
                options.ServiceName);
        }

        wolverine.LocalQueue("default")
            .Sequential();
    }

    private static TimeSpan[] GenerateRetryDelays(int maxRetries, int initialDelaySeconds)
    {
        if (maxRetries <= 0)
        {
            return Array.Empty<TimeSpan>();
        }

        var delays = new TimeSpan[maxRetries];
        for (var i = 0; i < maxRetries; i++)
        {
            var delaySeconds = Math.Min(initialDelaySeconds * Math.Pow(2, i), 60);
            delays[i] = TimeSpan.FromSeconds(delaySeconds);
        }

        return delays;
    }
}
