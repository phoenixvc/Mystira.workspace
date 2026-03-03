using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Contracts.App.Responses.Scenarios;

namespace Mystira.Application.CQRS.Scenarios.Queries;

/// <summary>
/// Wolverine handler for GetFeaturedScenariosQuery.
/// Returns scenarios marked as featured and active.
/// </summary>
public static class GetFeaturedScenariosQueryHandler
{
    /// <summary>
    /// Handles the GetFeaturedScenariosQuery by retrieving featured scenarios from the repository.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<List<ScenarioSummary>> Handle(
        GetFeaturedScenariosQuery query,
        IScenarioRepository repository,
        ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation("Retrieving featured scenarios");

        // Get featured and active scenarios mapped to ScenarioSummary
        var scenarios = await repository.GetQueryable()
            .Where(s => s.IsFeatured && s.IsActive)
            .OrderBy(s => s.Title)
            .Select(s => new ScenarioSummary
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                Tags = s.Tags,
                Difficulty = (int)s.Difficulty,
                SessionLength = (int)s.SessionLength,
                Archetypes = s.Archetypes,
                MinimumAge = s.MinimumAge,
                AgeGroup = s.AgeGroupId ?? string.Empty,
                CoreAxes = s.CoreAxes,
                CreatedAt = s.CreatedAt,
                Image = s.CoverImageUrl,
                MusicPalette = s.MusicPalette != null ? s.MusicPalette.DefaultMood : null,
                IsFeatured = s.IsFeatured,
                ThumbnailUrl = s.ThumbnailUrl
            })
            .ToListAsync(ct);

        logger.LogInformation("Found {Count} featured scenarios", scenarios.Count);

        return scenarios;
    }
}
