using Mystira.App.Api.Adapters;
using Mystira.App.Api.Configuration;
using Mystira.App.Api.Services;
using Mystira.App.Application;
using Mystira.App.Application.Ports;
using Mystira.App.Application.Ports.Media;
using Mystira.App.Application.Ports.Messaging;
using Mystira.App.Infrastructure.Azure;
using Mystira.App.Infrastructure.Azure.HealthChecks;
using Mystira.App.Infrastructure.Azure.Services;
using Mystira.App.Infrastructure.Data.Caching;
using Mystira.App.Infrastructure.Data.Services;
using Mystira.App.Infrastructure.Discord;
using Mystira.App.Infrastructure.Discord.Services;
using Mystira.App.Infrastructure.Chain;
using Mystira.App.Infrastructure.Payments;
using Mystira.Contracts.App.Ports.Health;
using System.IO.Compression;
using Mystira.Shared.Configuration;
using Mystira.Shared.Locking;
using Mystira.Shared.Middleware;
using Mystira.Shared.Telemetry;
using OpenTelemetry.Resources;
using Serilog;
using Serilog.Events;
using Wolverine;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Mystira.App.Api");

    var builder = WebApplication.CreateBuilder(args);

    // Configuration sources
    builder.Configuration.AddKeyVaultConfiguration(builder.Environment);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithCorrelationId()
        .Enrich.WithProperty("Application", "Mystira.App.Api")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.ApplicationInsights(
            services.GetService<TelemetryConfiguration>(),
            TelemetryConverter.Traces));

    // Telemetry & Observability
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.EnableDependencyTrackingTelemetryModule = true;
        options.EnableQuickPulseMetricStream = true;
        options.EnablePerformanceCounterCollectionModule = true;
        options.EnableRequestTrackingTelemetryModule = true;
    });

    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(serviceName: "Mystira.App.Api")
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = builder.Environment.EnvironmentName
            }));

    builder.Services.AddCustomMetrics(builder.Environment.EnvironmentName);
    builder.Services.AddSecurityMetrics(builder.Environment.EnvironmentName);
    builder.Services.AddUserJourneyAnalytics(builder.Environment.EnvironmentName);
    builder.Services.Configure<RequestLoggingOptions>(builder.Configuration.GetSection("RequestLogging"));

    // Controllers & JSON
    builder.Services.AddControllersWithViews()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });
    builder.Services.AddHttpClient()
        .ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler(options =>
            {
                // Retry: 3 attempts with exponential backoff (2s, 4s, 8s)
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(2);
                options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;

                // Circuit breaker: open after 50% failure rate over 30s window
                options.CircuitBreaker.FailureRatio = 0.5;
                options.CircuitBreaker.MinimumThroughput = 5;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);

                // Total request timeout: 30 seconds
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);

                // Per-attempt timeout: 10 seconds
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            });
        });

    var identityApiBaseUrl = builder.Configuration["IdentityApi:BaseUrl"] ?? "http://localhost:7100";
    builder.Services.AddHttpClient("IdentityAuth", client =>
    {
        client.BaseAddress = new Uri(identityApiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    });
    builder.Services.AddScoped<IIdentityAuthGateway, IdentityAuthGateway>();

    // Swagger, Database, Auth, Repos, UseCases, CORS, Rate Limiting
    builder.Services.AddMystiraSwagger();
    var (useCosmosDb, usePostgres) = builder.Services.AddMystiraDatabase(builder.Configuration);

    builder.Services.AddAzureBlobStorage(builder.Configuration);
    builder.Services.AddAzureEmailService(builder.Configuration);
    builder.Services.Configure<AudioTranscodingOptions>(builder.Configuration.GetSection(AudioTranscodingOptions.SectionName));
    builder.Services.AddSingleton<IAudioTranscodingService, FfmpegAudioTranscodingService>();
    builder.Services.AddSingleton<Mystira.App.Application.Services.ConsentEmailBuilder>();
    builder.Services.AddSingleton<Mystira.App.Application.Services.MagicSignupEmailBuilder>();
    builder.Services.AddScoped<IApiTokenService, ApiTokenService>();
    builder.Services.AddPaymentServices(builder.Configuration);

    builder.Services.AddMystiraAuthentication(builder.Configuration, builder.Environment);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<Mystira.App.Application.Ports.Services.ICurrentUserService, CurrentUserService>();

    builder.Services.AddMystiraRepositories();
    builder.Services.AddScoped<MasterDataSeederService>();

    // Application layer services (use cases, validators, application services)
    builder.Services.AddApplicationServices();

    // Story Protocol / Chain service (feature flag: stub vs gRPC)
    builder.Services.AddChainServices(builder.Configuration);

    // COPPA data deletion service + background processor
    builder.Services.AddScoped<Mystira.App.Application.Ports.IDataDeletionService, Mystira.App.Application.Services.DataDeletionService>();
    builder.Services.AddHostedService<Mystira.App.Api.Services.DataDeletionBackgroundService>();

    // Infrastructure adapters registered at host level
    builder.Services.AddScoped<IHealthCheckService, HealthCheckServiceAdapter>();
    builder.Services.AddScoped<IHealthCheckPort, HealthCheckPortAdapter>();
    builder.Services.AddScoped<IMediaMetadataService, MediaMetadataService>();

    // Caching
    builder.Services.AddMemoryCache(options =>
    {
        options.SizeLimit = 1024;
        options.CompactionPercentage = 0.25;
    });
    builder.Services.AddRedisCaching(builder.Configuration);

    // Distributed locking (requires Redis)
    builder.Services.AddMystiraDistributedLocking(builder.Configuration);

    // Distributed tracing & Wolverine
    builder.Services.AddDistributedTracing("Mystira.App.Api", builder.Environment.EnvironmentName);
    builder.Host.UseWolverine(opts =>
    {
        // Discover handlers in the Application assembly (where all CQRS handlers live)
        opts.Discovery.IncludeAssembly(typeof(Mystira.App.Application.CQRS.Accounts.Queries.GetAccountQuery).Assembly);
        opts.Policies.UseDurableLocalQueues();
    });

    // Exception handling
    builder.Services.AddExceptionHandler<Mystira.App.Api.Middleware.GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Health checks
    var healthChecksBuilder = builder.Services.AddHealthChecks()
        .AddCheck<BlobStorageHealthCheck>("blob_storage")
        .AddPaymentServiceHealthCheck();

    if (useCosmosDb)
    {
        healthChecksBuilder.AddCheck<CosmosDbHealthCheck>("cosmos_db", tags: new[] { "ready", "db" });
    }

    var postgresConnectionString = builder.Configuration.GetConnectionString("PostgreSql");
    if (usePostgres)
    {
        healthChecksBuilder.AddNpgSql(
            postgresConnectionString!,
            name: "postgresql",
            tags: new[] { "ready", "db", "polyglot" });
    }

    // Discord (optional)
    var discordEnabled = builder.Configuration.GetValue<bool>("Discord:Enabled", false);
    if (discordEnabled)
    {
        builder.Services.AddDiscordBot(builder.Configuration);
        builder.Services.AddDiscordBotHostedService();
        builder.Services.AddHealthChecks().AddDiscordBotHealthCheck();
    }
    else
    {
        builder.Services.AddSingleton<NoOpChatBotService>();
        builder.Services.AddSingleton<IChatBotService>(sp => sp.GetRequiredService<NoOpChatBotService>());
        builder.Services.AddSingleton<IMessagingService>(sp => sp.GetRequiredService<NoOpChatBotService>());
        builder.Services.AddSingleton<IBotCommandService>(sp => sp.GetRequiredService<NoOpChatBotService>());
    }

    builder.Services.AddMystiraCors(builder.Configuration, builder.Environment);

    builder.Logging.AddConsole();
#if !DEBUG
    builder.Logging.AddAzureWebAppDiagnostics();
#endif

    builder.Services.AddMystiraRateLimiting();

    // Response compression (CDN/PERF-4)
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    });
    builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Fastest;
    });

    // Output caching for API responses
    builder.Services.AddOutputCache(options =>
    {
        // Default: no caching
        options.AddBasePolicy(builder => builder.NoCache());
        // Static/reference data: cache 5 minutes
        options.AddPolicy("ReferenceData", builder => builder.Expire(TimeSpan.FromMinutes(5)));
        // Media metadata: cache 1 hour
        options.AddPolicy("MediaCache", builder => builder.Expire(TimeSpan.FromHours(1)));
    });

    // ═══════════════════════════════════════════════════════════════════════════════
    // BUILD & CONFIGURE HTTP PIPELINE
    // ═══════════════════════════════════════════════════════════════════════════════
    var app = builder.Build();

    var logger = app.Logger;
    logger.LogInformation(useCosmosDb ? "Using Azure Cosmos DB (Cloud Database)" : "Using In-Memory Database (Local Development)");
    logger.LogInformation(discordEnabled ? "Discord bot integration: ENABLED" : "Discord bot integration: DISABLED");

    if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mystira API v1");
            c.RoutePrefix = string.Empty;
        });
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseExceptionHandler();
    app.UseResponseCompression();
    app.UseSecurityHeaders();
    app.UseRateLimiter();
    app.UseCorrelationId();

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? string.Empty);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? string.Empty);
            if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId))
            {
                diagnosticContext.Set("CorrelationId", correlationId ?? string.Empty);
            }
        };
    });

    app.UseRequestLogging();
    app.UseRouting();
    app.UseCors(CorsExtensions.PolicyName);
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseOutputCache();
    app.MapControllers();

    // Health endpoints
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            });
            await context.Response.WriteAsync(result);
        }
    });
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false
    });

    // Database initialization
    await app.InitializeDatabaseAsync(builder.Configuration, useCosmosDb);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for testing
namespace Mystira.App.Api
{
    public partial class Program { }
}
