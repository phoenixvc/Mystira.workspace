# Distributed Tracing Extensions

**Status**: Implementation Ready
**Version**: 1.0
**Last Updated**: 2025-12-22
**Phase**: 3.0 - Monitoring & Observability

## Overview

This document provides the implementation template for W3C Trace Context compliant distributed tracing across all Mystira services.

## NuGet Package Requirements

```xml
<PackageReference Include="OpenTelemetry" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.0.0-beta.9" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.7.0" />
<PackageReference Include="Azure.Monitor.OpenTelemetry.Exporter" Version="1.2.0" />
```

## DistributedTracingExtensions.cs

```csharp
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Mystira.Infrastructure.Telemetry;

/// <summary>
/// Extension methods for configuring W3C Trace Context compliant distributed tracing.
/// </summary>
public static class DistributedTracingExtensions
{
    /// <summary>
    /// Activity source for creating custom spans.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new("Mystira.Platform");

    /// <summary>
    /// Adds OpenTelemetry distributed tracing with W3C Trace Context propagation.
    /// </summary>
    public static IServiceCollection AddDistributedTracing(
        this IServiceCollection services,
        string serviceName,
        string serviceVersion,
        Action<TracerProviderBuilder>? configureTracing = null)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: serviceName,
                serviceVersion: serviceVersion,
                serviceInstanceId: Environment.MachineName)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                ["service.namespace"] = "mystira",
                ["cloud.provider"] = "azure",
                ["cloud.region"] = Environment.GetEnvironmentVariable("AZURE_REGION") ?? "unknown"
            });

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddDetector(new ResourceDetectorAdapter(resourceBuilder)))
            .WithTracing(builder =>
            {
                builder
                    // Instrumentation
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = EnrichWithHttpRequest;
                        options.EnrichWithHttpResponse = EnrichWithHttpResponse;
                        options.Filter = FilterHealthChecks;
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequestMessage = EnrichOutgoingRequest;
                        options.EnrichWithHttpResponseMessage = EnrichOutgoingResponse;
                    })
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;
                    })
                    .AddSource(ActivitySource.Name)
                    // W3C Trace Context propagation (default)
                    .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(1.0)));

                // Azure Monitor exporter
                var connectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    builder.AddAzureMonitorTraceExporter(options =>
                    {
                        options.ConnectionString = connectionString;
                    });
                }

                // OTLP exporter for local development
                var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    builder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
                }

                // Console exporter for development
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    builder.AddConsoleExporter();
                }

                configureTracing?.Invoke(builder);
            })
            .WithMetrics(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation();

                var connectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    builder.AddAzureMonitorMetricExporter(options =>
                    {
                        options.ConnectionString = connectionString;
                    });
                }
            });

        // Add logging integration
        services.AddLogging(logging =>
        {
            logging.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resourceBuilder);
                options.IncludeScopes = true;
                options.IncludeFormattedMessage = true;
                options.ParseStateValues = true;

                var connectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    options.AddAzureMonitorLogExporter(exporterOptions =>
                    {
                        exporterOptions.ConnectionString = connectionString;
                    });
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Adds trace context middleware for propagating trace IDs in responses.
    /// </summary>
    public static IApplicationBuilder UseTraceContextPropagation(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var activity = Activity.Current;
            if (activity != null)
            {
                // Add trace ID to response headers for debugging
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers.TryAdd("X-Trace-Id", activity.TraceId.ToString());
                    context.Response.Headers.TryAdd("X-Span-Id", activity.SpanId.ToString());
                    return Task.CompletedTask;
                });
            }

            await next();
        });
    }

    #region Enrichment Methods

    private static void EnrichWithHttpRequest(Activity activity, HttpRequest request)
    {
        activity.SetTag("http.client_ip", request.HttpContext.Connection.RemoteIpAddress?.ToString());
        activity.SetTag("http.request_content_length", request.ContentLength);

        // Add correlation ID if present
        if (request.Headers.TryGetValue("X-Correlation-Id", out var correlationId))
        {
            activity.SetTag("correlation.id", correlationId.ToString());
        }

        // Add user context if authenticated
        if (request.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            activity.SetTag("user.id", request.HttpContext.User.FindFirst("sub")?.Value);
            activity.SetTag("user.authenticated", true);
        }
    }

    private static void EnrichWithHttpResponse(Activity activity, HttpResponse response)
    {
        activity.SetTag("http.response_content_length", response.ContentLength);

        // Mark errors
        if (response.StatusCode >= 400)
        {
            activity.SetStatus(ActivityStatusCode.Error);
            activity.SetTag("error", true);
            activity.SetTag("error.type", response.StatusCode >= 500 ? "server_error" : "client_error");
        }
    }

    private static void EnrichOutgoingRequest(Activity activity, HttpRequestMessage request)
    {
        activity.SetTag("http.request.method", request.Method.Method);
        activity.SetTag("peer.service", request.RequestUri?.Host);
    }

    private static void EnrichOutgoingResponse(Activity activity, HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            activity.SetStatus(ActivityStatusCode.Error);
            activity.SetTag("error", true);
        }
    }

    private static bool FilterHealthChecks(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();
        return !string.IsNullOrEmpty(path) &&
               !path.Contains("/health") &&
               !path.Contains("/ready") &&
               !path.Contains("/live") &&
               !path.Contains("/metrics");
    }

    #endregion

    #region Custom Span Helpers

    /// <summary>
    /// Creates a new activity (span) for custom operations.
    /// </summary>
    public static Activity? StartActivity(
        string operationName,
        ActivityKind kind = ActivityKind.Internal,
        IDictionary<string, object?>? tags = null)
    {
        var activity = ActivitySource.StartActivity(operationName, kind);

        if (activity != null && tags != null)
        {
            foreach (var (key, value) in tags)
            {
                activity.SetTag(key, value);
            }
        }

        return activity;
    }

    /// <summary>
    /// Creates a span for database operations.
    /// </summary>
    public static Activity? StartDatabaseSpan(
        string operation,
        string? table = null,
        string? statement = null)
    {
        var activity = ActivitySource.StartActivity(
            $"db.{operation}",
            ActivityKind.Client);

        if (activity != null)
        {
            activity.SetTag("db.system", "postgresql");
            activity.SetTag("db.operation", operation);

            if (!string.IsNullOrEmpty(table))
                activity.SetTag("db.sql.table", table);

            if (!string.IsNullOrEmpty(statement))
                activity.SetTag("db.statement", statement);
        }

        return activity;
    }

    /// <summary>
    /// Creates a span for external service calls.
    /// </summary>
    public static Activity? StartExternalServiceSpan(
        string serviceName,
        string operation)
    {
        var activity = ActivitySource.StartActivity(
            $"{serviceName}.{operation}",
            ActivityKind.Client);

        if (activity != null)
        {
            activity.SetTag("peer.service", serviceName);
            activity.SetTag("rpc.method", operation);
        }

        return activity;
    }

    /// <summary>
    /// Creates a span for message processing.
    /// </summary>
    public static Activity? StartMessageProcessingSpan(
        string messageType,
        string? messageId = null)
    {
        var activity = ActivitySource.StartActivity(
            $"process.{messageType}",
            ActivityKind.Consumer);

        if (activity != null)
        {
            activity.SetTag("messaging.system", "azure_servicebus");
            activity.SetTag("messaging.operation", "process");
            activity.SetTag("messaging.message.type", messageType);

            if (!string.IsNullOrEmpty(messageId))
                activity.SetTag("messaging.message.id", messageId);
        }

        return activity;
    }

    #endregion
}

/// <summary>
/// Adapter for ResourceBuilder to implement IResourceDetector.
/// </summary>
internal sealed class ResourceDetectorAdapter : IResourceDetector
{
    private readonly ResourceBuilder _builder;

    public ResourceDetectorAdapter(ResourceBuilder builder)
    {
        _builder = builder;
    }

    public Resource Detect() => _builder.Build();
}
```

## Service Registration

### Program.cs Integration

```csharp
using Mystira.Infrastructure.Telemetry;

var builder = WebApplication.CreateBuilder(args);

// Add distributed tracing
builder.Services.AddDistributedTracing(
    serviceName: "mystira-app-api",
    serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0",
    configureTracing: tracing =>
    {
        // Add custom instrumentation if needed
        tracing.AddSource("Mystira.GameSession");
        tracing.AddSource("Mystira.StoryGeneration");
    });

var app = builder.Build();

// Add trace context propagation middleware
app.UseTraceContextPropagation();

app.Run();
```

## Usage Examples

### Creating Custom Spans

```csharp
public class GameSessionService
{
    public async Task<GameSession> StartSessionAsync(string accountId, string scenarioId)
    {
        using var activity = DistributedTracingExtensions.StartActivity(
            "GameSession.Start",
            ActivityKind.Internal,
            new Dictionary<string, object?>
            {
                ["account.id"] = accountId,
                ["scenario.id"] = scenarioId
            });

        try
        {
            var session = await CreateSessionAsync(accountId, scenarioId);

            activity?.SetTag("session.id", session.Id);
            activity?.SetStatus(ActivityStatusCode.Ok);

            return session;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }
}
```

### Database Operation Tracing

```csharp
public class AccountRepository
{
    public async Task<Account?> FindByEmailAsync(string email)
    {
        using var activity = DistributedTracingExtensions.StartDatabaseSpan(
            operation: "SELECT",
            table: "accounts",
            statement: "SELECT * FROM accounts WHERE email = @email");

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Email == email);

        activity?.SetTag("db.result.count", account != null ? 1 : 0);

        return account;
    }
}
```

### External Service Call Tracing

```csharp
public class StoryGeneratorClient
{
    public async Task<StoryResponse> GenerateStoryAsync(StoryRequest request)
    {
        using var activity = DistributedTracingExtensions.StartExternalServiceSpan(
            serviceName: "story-generator",
            operation: "GenerateStory");

        activity?.SetTag("story.scenario_id", request.ScenarioId);
        activity?.SetTag("story.prompt_length", request.Prompt.Length);

        var response = await _httpClient.PostAsJsonAsync("/api/generate", request);

        activity?.SetTag("story.response_length", response.Content.Headers.ContentLength);
        activity?.SetTag("http.status_code", (int)response.StatusCode);

        return await response.Content.ReadFromJsonAsync<StoryResponse>()
            ?? throw new InvalidOperationException("Empty response");
    }
}
```

## W3C Trace Context Headers

The implementation automatically propagates these headers:

| Header | Description | Example |
|--------|-------------|---------|
| `traceparent` | W3C Trace Context parent | `00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01` |
| `tracestate` | W3C Trace Context state | `azure=xxx` |
| `X-Trace-Id` | Trace ID (response header) | `4bf92f3577b34da6a3ce929d0e0e4736` |
| `X-Span-Id` | Span ID (response header) | `00f067aa0ba902b7` |

## Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Yes (prod) | Azure Application Insights connection |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | No | OTLP endpoint for local collectors |
| `ASPNETCORE_ENVIRONMENT` | No | Environment name for resource attributes |
| `AZURE_REGION` | No | Azure region for resource attributes |

## Integration Checklist

- [ ] Add OpenTelemetry NuGet packages
- [ ] Create DistributedTracingExtensions.cs
- [ ] Register in Program.cs
- [ ] Add trace context middleware
- [ ] Configure Application Insights connection string
- [ ] Add custom spans for business operations
- [ ] Verify traces appear in Azure Monitor
- [ ] Test cross-service trace correlation

## References

- [W3C Trace Context Specification](https://www.w3.org/TR/trace-context/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
- [Azure Monitor OpenTelemetry](https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-enable)
- [SLO Definitions](../../operations/slo-definitions.md)
