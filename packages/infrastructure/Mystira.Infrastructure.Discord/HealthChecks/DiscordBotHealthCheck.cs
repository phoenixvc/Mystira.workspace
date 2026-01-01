using Microsoft.Extensions.Diagnostics.HealthChecks;
using Mystira.Infrastructure.Discord.Services;

namespace Mystira.Infrastructure.Discord.HealthChecks;

/// <summary>
/// Health check to verify Discord bot connectivity.
/// Uses concrete DiscordBotService to ensure correct service is injected in multi-platform setups.
/// </summary>
public class DiscordBotHealthCheck : IHealthCheck
{
    private readonly DiscordBotService _discordBotService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscordBotHealthCheck"/> class.
    /// </summary>
    /// <param name="discordBotService">The Discord bot service instance.</param>
    public DiscordBotHealthCheck(DiscordBotService discordBotService)
    {
        _discordBotService = discordBotService;
    }

    /// <summary>
    /// Checks the health status of the Discord bot.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The health check result indicating the bot's status.</returns>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var status = _discordBotService.GetStatus();

            if (!status.IsConnected)
            {
                return Task.FromResult(
                    HealthCheckResult.Unhealthy(
                        "Discord bot is not connected",
                        data: new Dictionary<string, object>
                        {
                            ["IsConnected"] = false,
                            ["IsEnabled"] = status.IsEnabled
                        }));
            }

            if (string.IsNullOrEmpty(status.BotName))
            {
                return Task.FromResult(
                    HealthCheckResult.Degraded(
                        "Discord bot is connected but user information is not available",
                        data: new Dictionary<string, object>
                        {
                            ["IsConnected"] = true,
                            ["HasUserInfo"] = false
                        }));
            }

            var data = new Dictionary<string, object>
            {
                ["IsConnected"] = true,
                ["BotUsername"] = status.BotName,
                ["ServerCount"] = status.ServerCount
            };

            if (status.BotId.HasValue)
            {
                data["BotId"] = status.BotId.Value;
            }

            return Task.FromResult(
                HealthCheckResult.Healthy(
                    "Discord bot is connected and operational",
                    data: data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "Discord bot health check failed",
                    exception: ex));
        }
    }
}
