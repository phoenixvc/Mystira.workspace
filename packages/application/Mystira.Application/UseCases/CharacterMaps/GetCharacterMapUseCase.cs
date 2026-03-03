using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.CharacterMaps;

/// <summary>
/// Use case for retrieving a character map by ID
/// </summary>
public class GetCharacterMapUseCase
{
    private readonly ICharacterMapRepository _repository;
    private readonly ILogger<GetCharacterMapUseCase> _logger;

    /// <summary>Initializes a new instance of the <see cref="GetCharacterMapUseCase"/> class.</summary>
    /// <param name="repository">The character map repository.</param>
    /// <param name="logger">The logger.</param>
    public GetCharacterMapUseCase(
        ICharacterMapRepository repository,
        ILogger<GetCharacterMapUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>Retrieves a character map by its identifier.</summary>
    /// <param name="characterMapId">The character map identifier.</param>
    /// <returns>The character map if found; otherwise, null.</returns>
    public async Task<CharacterMap?> ExecuteAsync(string characterMapId)
    {
        if (string.IsNullOrWhiteSpace(characterMapId))
        {
            throw new ArgumentException("Character map ID cannot be null or empty", nameof(characterMapId));
        }

        var characterMap = await _repository.GetByIdAsync(characterMapId);

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

