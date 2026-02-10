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

            options.AddFixedWindowLimiter("auth", limiterOptions =>
            {
                limiterOptions.PermitLimit = 5;
                limiterOptions.Window = TimeSpan.FromMinutes(15);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
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
