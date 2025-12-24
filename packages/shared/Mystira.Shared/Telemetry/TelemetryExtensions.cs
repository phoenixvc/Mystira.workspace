using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace Mystira.Shared.Telemetry;

/// <summary>
/// Extension methods for configuring Mystira telemetry.
/// </summary>
public static class TelemetryExtensions
{
    /// <summary>
    /// Adds Mystira activity source to the OpenTelemetry tracer provider.
    /// </summary>
    /// <param name="builder">The tracer provider builder.</param>
    /// <returns>The tracer provider builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddOpenTelemetry()
    ///     .WithTracing(tracing => tracing
    ///         .AddMystiraInstrumentation()
    ///         .AddAspNetCoreInstrumentation()
    ///         .AddHttpClientInstrumentation());
    /// </code>
    /// </example>
    public static TracerProviderBuilder AddMystiraInstrumentation(this TracerProviderBuilder builder)
    {
        return builder.AddSource(MystiraActivitySource.Name);
    }
}
