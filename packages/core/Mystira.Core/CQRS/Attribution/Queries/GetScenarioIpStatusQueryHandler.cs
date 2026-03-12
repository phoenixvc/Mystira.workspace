using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.Core.Configuration.StoryProtocol;
using Mystira.Core.Ports.Data;
using Mystira.Contracts.App.Responses.Attribution;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;

namespace Mystira.Core.CQRS.Attribution.Queries;

/// <summary>
/// Wolverine handler for GetScenarioIpStatusQuery - retrieves IP registration status for a scenario
/// </summary>
public static class GetScenarioIpStatusQueryHandler
{
    public static async Task<IpVerificationResponse?> Handle(
        GetScenarioIpStatusQuery request,
        IScenarioRepository repository,
        ILogger<GetScenarioIpStatusQuery> logger,
        IOptions<StoryProtocolOptions> options,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ScenarioId))
        {
            throw new ValidationException("scenarioId", "Scenario ID cannot be null or empty");
        }

        var scenario = await repository.GetByIdAsync(request.ScenarioId, ct);

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
