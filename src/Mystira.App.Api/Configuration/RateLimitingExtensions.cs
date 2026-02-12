using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Mystira.App.Application.Helpers;
using Mystira.Shared.Telemetry;

namespace Mystira.App.Api.Configuration;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddMystiraRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            // Auth-specific limiter: 5 requests per 15 minutes per IP
            options.AddPolicy("auth", context =>
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(15),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            // COPPA consent endpoint: 10 requests per 15 minutes per IP
            // Prevents email spam abuse on unauthenticated consent request endpoint
            options.AddPolicy("coppa", context =>
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetSlidingWindowLimiter(ipAddress, _ =>
                    new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(15),
                        SegmentsPerWindow = 3,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                var securityMetrics = context.HttpContext.RequestServices.GetService<ISecurityMetrics>();
                var clientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString();
                var endpoint = context.HttpContext.Request.Path.Value ?? "unknown";
                securityMetrics?.TrackRateLimitHit(LogAnonymizer.HashId(clientIp), endpoint);

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsync(
                    "Too many requests. Please try again later.",
                    cancellationToken);
            };
        });

        return services;
    }
}
