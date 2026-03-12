using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Contracts.App.Requests.Contributors;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.Contributors;

/// <summary>
/// Use case for setting contributors for a scenario
/// </summary>
public class SetScenarioContributorsUseCase
{
    private readonly IScenarioRepository _scenarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetScenarioContributorsUseCase> _logger;

    public SetScenarioContributorsUseCase(
        IScenarioRepository scenarioRepository,
        IUnitOfWork unitOfWork,
        ILogger<SetScenarioContributorsUseCase> logger)
    {
        _scenarioRepository = scenarioRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ScenarioStoryProtocol> ExecuteAsync(string scenarioId, SetContributorsRequest request, CancellationToken ct = default)
    {
        // Get the scenario
        var scenario = await _scenarioRepository.GetByIdAsync(scenarioId, ct);
        if (scenario == null)
        {
            throw new NotFoundException("Scenario", scenarioId);
        }

        // Convert request to domain models
        var contributors = request.Contributors.Select(c => new Contributor
        {
            Id = Guid.NewGuid().ToString(),
            Name = c.Name,
            WalletAddress = c.WalletAddress ?? string.Empty,
            Role = (ContributorRole)(int)c.Role,
            ContributionPercentage = c.ContributionPercentage,
            Email = c.Email,
            Notes = c.Notes,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        // Initialize Story Protocol metadata if it doesn't exist
        if (scenario.StoryProtocol == null)
        {
            scenario.StoryProtocol = new ScenarioStoryProtocol();
        }

        // Set the contributors
        scenario.StoryProtocol.Contributors = contributors;

        // Validate contributor splits
        if (!scenario.StoryProtocol.ValidateContributorSplits(out var errors))
        {
            var errorMessage = string.Join("; ", errors);
            _logger.LogWarning("Invalid contributor splits for scenario {ScenarioId}: {Errors}", scenarioId, errorMessage);
            throw new ValidationException("contributors", $"Invalid contributor configuration: {errorMessage}");
        }

        // Update the scenario
        await _scenarioRepository.UpdateAsync(scenario, ct);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving contributors for scenario: {ScenarioId}", scenarioId);
            throw;
        }

        _logger.LogInformation("Set {Count} contributors for scenario: {ScenarioId}", contributors.Count, scenarioId);
        return scenario.StoryProtocol;
    }
}
