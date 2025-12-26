using Microsoft.AspNetCore.SignalR;
using Mystira.App.Admin.Api.Hubs;
using Mystira.App.Admin.Api.Services;

namespace Mystira.App.Admin.Api.Configuration;

/// <summary>
/// Extension methods for configuring SignalR in the application.
/// </summary>
public static class SignalRConfiguration
{
    /// <summary>
    /// Adds SignalR services with optional Redis backplane for scaling.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration</param>
    /// <param name="environment">The hosting environment</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMystiraSignalR(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var signalRBuilder = services.AddSignalR(options =>
        {
            // Configure timeouts
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.HandshakeTimeout = TimeSpan.FromSeconds(15);
            
            // Enable detailed errors only in development
            options.EnableDetailedErrors = environment.IsDevelopment();
            
            // Limit message size to prevent abuse (1 MB default)
            options.MaximumReceiveMessageSize = 1024 * 1024;
            
            // Configure parallelism
            options.MaximumParallelInvocationsPerClient = 1;
            
            // Enable stateful reconnect (requires client support)
            options.StatefulReconnectBufferSize = 1024 * 1024; // 1 MB
        });

        // Use Redis backplane if connection string is provided
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            signalRBuilder.AddStackExchangeRedis(redisConnectionString, options =>
            {
                options.Configuration.ChannelPrefix = "mystira:signalr:";
                options.Configuration.AbortOnConnectFail = false; // Continue without Redis if it's down
            });
        }

        // Register event notification service
        services.AddSingleton<IEventNotificationService, EventNotificationService>();

        return services;
    }

    /// <summary>
    /// Maps SignalR hubs to the application endpoints.
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>The web application for chaining</returns>
    public static WebApplication MapMystiraSignalRHubs(this WebApplication app)
    {
        // Map the events hub
        app.MapHub<EventsHub>("/hubs/events", options =>
        {
            // Configure transport options
            options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets
                               | Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents
                               | Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
            
            // Set WebSocket as preferred
            options.TransportMaxBufferSize = 1024 * 1024; // 1 MB
            options.ApplicationMaxBufferSize = 1024 * 1024; // 1 MB
            
            // Configure long polling timeout
            options.LongPolling = new Microsoft.AspNetCore.Http.Connections.LongPollingOptions
            {
                PollTimeout = TimeSpan.FromSeconds(90)
            };
        });

        return app;
    }

    /// <summary>
    /// Adds CORS policy that supports SignalR requirements.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSignalRCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("SignalR:AllowedOrigins").Get<string[]>()
                           ?? new[]
                           {
                               "http://localhost:3000",
                               "http://localhost:5173",
                               "https://localhost:3000",
                               "https://localhost:5173"
                           };

        services.AddCors(options =>
        {
            options.AddPolicy("SignalRPolicy", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials(); // Required for SignalR
            });
        });

        return services;
    }
}
