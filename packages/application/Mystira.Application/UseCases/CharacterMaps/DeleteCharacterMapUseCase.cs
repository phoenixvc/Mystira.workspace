using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;

namespace Mystira.Application.UseCases.CharacterMaps;

/// <summary>
/// Use case for deleting a character map
/// </summary>
public class DeleteCharacterMapUseCase
{
    private readonly ICharacterMapRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCharacterMapUseCase> _logger;

    /// <summary>Initializes a new instance of the <see cref="DeleteCharacterMapUseCase"/> class.</summary>
    /// <param name="repository">The character map repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public DeleteCharacterMapUseCase(
        ICharacterMapRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteCharacterMapUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>Deletes a character map by its identifier.</summary>
    /// <param name="characterMapId">The character map identifier.</param>
    /// <returns>True if the character map was deleted successfully; otherwise, false.</returns>
    public async Task<bool> ExecuteAsync(string characterMapId)
    {
        if (string.IsNullOrWhiteSpace(characterMapId))
        {
            throw new ArgumentException("Character map ID cannot be null or empty", nameof(characterMapId));
        }

        var characterMap = await _repository.GetByIdAsync(characterMapId);
        if (characterMap == null)
        {
            _logger.LogWarning("Character map not found for deletion: {CharacterMapId}", characterMapId);
            return false;
        }

        await _repository.DeleteAsync(characterMapId);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted character map: {CharacterMapId}", characterMapId);
        return true;
    }
}

