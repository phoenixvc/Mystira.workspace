using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using System.Threading;

namespace Mystira.App.Application.UseCases.CharacterMaps;

/// <summary>
/// Use case for retrieving all character maps
/// </summary>
public class GetCharacterMapsUseCase
{
    private readonly ICharacterMapRepository _repository;
    private readonly ILogger<GetCharacterMapsUseCase> _logger;

    public GetCharacterMapsUseCase(
        ICharacterMapRepository repository,
        ILogger<GetCharacterMapsUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<CharacterMap>> ExecuteAsync(CancellationToken ct = default)
    {
        var characterMaps = await _repository.GetAllAsync(ct);
        var characterMapList = characterMaps.ToList();

        _logger.LogInformation("Retrieved {Count} character maps", characterMapList.Count);
        return characterMapList;
    }
}

