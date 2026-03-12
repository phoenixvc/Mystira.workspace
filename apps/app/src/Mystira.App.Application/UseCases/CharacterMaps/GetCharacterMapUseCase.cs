using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.CharacterMaps;

/// <summary>
/// Use case for retrieving a character map by ID
/// </summary>
public class GetCharacterMapUseCase
{
    private readonly ICharacterMapRepository _repository;
    private readonly ILogger<GetCharacterMapUseCase> _logger;

    public GetCharacterMapUseCase(
        ICharacterMapRepository repository,
        ILogger<GetCharacterMapUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CharacterMap?> ExecuteAsync(string characterMapId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(characterMapId))
        {
            throw new ValidationException("characterMapId", "characterMapId is required");
        }

        var characterMap = await _repository.GetByIdAsync(characterMapId, ct);

        if (characterMap == null)
        {
            _logger.LogWarning("Character map not found: {CharacterMapId}", characterMapId);
        }
        else
        {
            _logger.LogDebug("Retrieved character map: {CharacterMapId}", characterMapId);
        }

        return characterMap;
    }
}

