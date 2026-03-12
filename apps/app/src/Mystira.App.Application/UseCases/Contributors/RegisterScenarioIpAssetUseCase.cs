using Microsoft.Extensions.Logging;
using Mystira.Core.Ports;
using Mystira.Core.Ports.Data;
using Mystira.App.Domain.Exceptions;
using Mystira.Contracts.App.Requests.Contributors;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using System.Threading;

namespace Mystira.App.Application.UseCases.Contributors;

/// <summary>
/// Use case for registering a scenario as an IP Asset on Story Protocol
/// </summary>
public class RegisterScenarioIpAssetUseCase
{
    private readonly IScenarioRepository _scenarioRepository;
    private readonly IStoryProtocolService _storyProtocolService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterScenarioIpAssetUseCase> _logger;

    public RegisterScenarioIpAssetUseCase(
        IScenarioRepository scenarioRepository,
        IStoryProtocolService storyProtocolService,
        IUnitOfWork unitOfWork,
        ILogger<RegisterScenarioIpAssetUseCase> logger)
    {
        _scenarioRepository = scenarioRepository;
        _storyProtocolService = storyProtocolService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ScenarioStoryProtocol> ExecuteAsync(string scenarioId, RegisterIpAssetRequest request, CancellationToken ct = default)
    {
        // Get the scenario
        var scenario = await _scenarioRepository.GetByIdAsync(scenarioId, ct);
        if (scenario == null)
        {
            throw new NotFoundException("Scenario", scenarioId);
        }

        // Check if already registered
        if (scenario.StoryProtocol?.IsRegistered ?? false)
        {
            throw new ConflictException("Scenario", $"Scenario {scenarioId} is already registered on Story Protocol");
        }

        // Ensure contributors are set
        if (scenario.StoryProtocol == null || !scenario.StoryProtocol.Contributors.Any())
        {
            throw new BusinessRuleException("ContributorsRequired", "Contributors must be set before registering on Story Protocol");
        }

        // Validate contributor splits
        if (!scenario.StoryProtocol.ValidateContributorSplits(out var errors))
        {
            var errorMessage = string.Join("; ", errors);
            throw new ValidationException("contributors", $"Invalid contributor configuration: {errorMessage}");
        }

        // Register on Story Protocol
        var storyProtocolMetadata = await _storyProtocolService.RegisterIpAssetAsync(
            scenario.Id,
            scenario.Title,
            scenario.StoryProtocol.Contributors,
            request.MetadataUri,
            request.LicenseTermsId,
            ct);

        // Update the scenario with Story Protocol metadata
        scenario.StoryProtocol.IpAssetId = storyProtocolMetadata.IpAssetId;
        scenario.StoryProtocol.RegistrationTxHash = storyProtocolMetadata.RegistrationTxHash;
        scenario.StoryProtocol.RegisteredAt = storyProtocolMetadata.RegisteredAt;
        scenario.StoryProtocol.IsRegistered = true;

        await _scenarioRepository.UpdateAsync(scenario, ct);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving Story Protocol registration for scenario: {ScenarioId}", scenarioId);
            throw;
        }

        _logger.LogInformation("Registered scenario {ScenarioId} as IP Asset: {IpAssetId}",
            scenarioId, storyProtocolMetadata.IpAssetId);

        return scenario.StoryProtocol;
    }
}
