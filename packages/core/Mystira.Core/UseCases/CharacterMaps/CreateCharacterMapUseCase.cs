using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Shared.Exceptions;
using Mystira.Contracts.App.Requests.CharacterMaps;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using System.Threading;

namespace Mystira.Core.UseCases.CharacterMaps;

/// <summary>
/// Use case for creating a new character map
/// </summary>
public class CreateCharacterMapUseCase
{
    private readonly ICharacterMapRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateCharacterMapUseCase> _logger;

    public CreateCharacterMapUseCase(
        ICharacterMapRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<CreateCharacterMapUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CharacterMap> ExecuteAsync(CreateCharacterMapRequest request, CancellationToken ct = default)
    {
        if (request == null)
        {
            throw new ValidationException("request", "request is required");
        }

        // Check if character map with ID already exists
        var existingCharacterMap = await _repository.GetByIdAsync(request.Id, ct);
        if (existingCharacterMap != null)
        {
            throw new ConflictException($"Character map with ID {request.Id} already exists");
        }

        var characterMap = new CharacterMap
        {
            Id = request.Id,
            Name = request.Name,
            Image = request.Image ?? string.Empty,
            Audio = request.Audio,
            Metadata = MapMetadata(request.Metadata),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(characterMap, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Created character map: {CharacterMapId} - {Name}", characterMap.Id, characterMap.Name);
        return characterMap;
    }

    private static CharacterMetadata MapMetadata(Dictionary<string, object>? metadata)
    {
        if (metadata == null)
        {
            return new CharacterMetadata();
        }

        return new CharacterMetadata
        {
            Roles = GetListFromMetadata(metadata, "roles"),
            Archetypes = GetListFromMetadata(metadata, "archetypes"),
            Species = metadata.TryGetValue("species", out var species) ? species?.ToString() ?? string.Empty : string.Empty,
            Age = metadata.TryGetValue("age", out var age) ? Convert.ToInt32(age) : 0,
            Traits = GetListFromMetadata(metadata, "traits"),
            Backstory = metadata.TryGetValue("backstory", out var backstory) ? backstory?.ToString() ?? string.Empty : string.Empty
        };
    }

    private static List<string> GetListFromMetadata(Dictionary<string, object> metadata, string key)
    {
        if (!metadata.TryGetValue(key, out var value) || value == null)
        {
            return new List<string>();
        }

        if (value is IEnumerable<object> enumerable)
        {
            return enumerable.Select(x => x?.ToString() ?? string.Empty).Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        if (value is IEnumerable<string> strings)
        {
            return strings.ToList();
        }

        return new List<string>();
    }
}

