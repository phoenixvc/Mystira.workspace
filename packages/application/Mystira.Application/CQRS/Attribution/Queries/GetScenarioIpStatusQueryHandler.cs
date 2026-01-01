using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.Application.Configuration.StoryProtocol;
using Mystira.Application.Ports.Data;
using Mystira.Contracts.App.Responses.Attribution;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Attribution.Queries;

/// <summary>
/// Wolverine handler for GetScenarioIpStatusQuery - retrieves IP registration status for a scenario
/// </summary>
public static class GetScenarioIpStatusQueryHandler
{
    /// <summary>
    /// Handles the GetScenarioIpStatusQuery.
    /// </summary>
    /// <param name="request">The query to handle.</param>
    /// <param name="repository">The scenario repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The Story Protocol configuration options.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The IP verification response if the scenario is found; otherwise, null.</returns>
    public static async Task<IpVerificationResponse?> Handle(
        GetScenarioIpStatusQuery request,
        IScenarioRepository repository,
        ILogger<GetScenarioIpStatusQuery> logger,
        IOptions<StoryProtocolOptions> options,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ScenarioId))
        {
            throw new ArgumentException("Scenario ID cannot be null or empty", nameof(request.ScenarioId));
        }

        var scenario = await repository.GetByIdAsync(request.ScenarioId);

        if (scenario == null)
        {
            logger.LogWarning("Scenario not found for IP status check: {ScenarioId}", request.ScenarioId);
            return null;
        }

        return MapToIpStatusResponse(scenario, options.Value);
    }

    private static IpVerificationResponse MapToIpStatusResponse(Scenario scenario, StoryProtocolOptions options)
    {
        var storyProtocol = scenario.StoryProtocol;
        var isRegistered = storyProtocol?.IsRegistered ?? false;

        return new IpVerificationResponse
        {
            ContentId = scenario.Id,
            ContentTitle = scenario.Title,
            IsRegistered = isRegistered,
            IpAssetId = storyProtocol?.IpAssetId,
            RegisteredAt = storyProtocol?.RegisteredAt,
            RegistrationTxHash = storyProtocol?.RegistrationTxHash,
            RoyaltyModuleId = storyProtocol?.RoyaltyModuleId,
            ContributorCount = storyProtocol?.Contributors?.Count ?? 0,
            ExplorerUrl = isRegistered && !string.IsNullOrEmpty(storyProtocol?.IpAssetId)
                ? $"{options.ExplorerBaseUrl}/address/{storyProtocol.IpAssetId}"
                : null
        };
    }
}
