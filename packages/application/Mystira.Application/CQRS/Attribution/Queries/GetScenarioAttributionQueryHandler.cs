using Microsoft.Extensions.Logging;
using Mystira.Application.Helpers;
using Mystira.Application.Ports.Data;
using Mystira.Contracts.App.Responses.Attribution;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Attribution.Queries;

/// <summary>
/// Wolverine handler for GetScenarioAttributionQuery - retrieves creator credits for a scenario
/// </summary>
public static class GetScenarioAttributionQueryHandler
{
    /// <summary>
    /// Handles the GetScenarioAttributionQuery.
    /// </summary>
    /// <param name="request">The query to handle.</param>
    /// <param name="repository">The scenario repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The content attribution response if the scenario is found; otherwise, null.</returns>
    public static async Task<ContentAttributionResponse?> Handle(
        GetScenarioAttributionQuery request,
        IScenarioRepository repository,
        ILogger<GetScenarioAttributionQuery> logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ScenarioId))
        {
            throw new ArgumentException("Scenario ID cannot be null or empty", nameof(request.ScenarioId));
        }

        var scenario = await repository.GetByIdAsync(request.ScenarioId);

        if (scenario == null)
        {
            logger.LogWarning("Scenario not found for attribution: {ScenarioId}", request.ScenarioId);
            return null;
        }

        return MapToAttributionResponse(scenario);
    }

    private static ContentAttributionResponse MapToAttributionResponse(Scenario scenario)
    {
        var response = new ContentAttributionResponse
        {
            ContentId = scenario.Id,
            ContentTitle = scenario.Title,
            IsIpRegistered = scenario.StoryProtocol?.IsRegistered ?? false,
            IpAssetId = scenario.StoryProtocol?.IpAssetId,
            RegisteredAt = scenario.StoryProtocol?.RegisteredAt,
            Credits = new List<CreatorCredit>()
        };

        if (scenario.StoryProtocol?.Contributors != null)
        {
            foreach (var contributor in scenario.StoryProtocol.Contributors)
            {
                response.Credits.Add(new CreatorCredit
                {
                    Name = contributor.Name,
                    Role = ContributorHelpers.GetRoleDisplayName(contributor.Role),
                    ContributionPercentage = contributor.ContributionPercentage
                });
            }
        }

        return response;
    }
}
