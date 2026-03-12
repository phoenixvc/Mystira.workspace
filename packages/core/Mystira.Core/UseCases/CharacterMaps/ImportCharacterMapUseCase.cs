using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using YamlDotNet.Serialization;
using System.Threading;

namespace Mystira.Core.UseCases.CharacterMaps;

/// <summary>
/// Use case for importing character maps from YAML format
/// </summary>
public class ImportCharacterMapUseCase
{
    private readonly ICharacterMapRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ImportCharacterMapUseCase> _logger;

    public ImportCharacterMapUseCase(
        ICharacterMapRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<ImportCharacterMapUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<CharacterMap>> ExecuteAsync(Stream yamlStream, CancellationToken ct = default)
    {
        if (yamlStream == null)
        {
            throw new ValidationException("yamlStream", "yamlStream is required");
        }

        var deserializer = new DeserializerBuilder()
            .WithCaseInsensitivePropertyMatching()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.UnderscoredNamingConvention.Instance)
            .Build();

        using var reader = new StreamReader(yamlStream);
        var yamlContent = await reader.ReadToEndAsync();

        var characterMapYaml = deserializer.Deserialize<CharacterMapYaml>(yamlContent);
        if (characterMapYaml?.Characters == null)
        {
            throw new ValidationException("input", "Invalid YAML format: missing characters array");
        }

        var importedCharacterMaps = new List<CharacterMap>();

        foreach (var yamlEntry in characterMapYaml.Characters)
        {
            var characterMap = new CharacterMap
            {
                Id = yamlEntry.Id,
                Name = yamlEntry.Name,
                Image = yamlEntry.Image,
                Audio = yamlEntry.Audio,
                Metadata = yamlEntry.Metadata ?? new CharacterMetadata(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Check if it exists and replace
            var existing = await _repository.GetByIdAsync(characterMap.Id, ct);
            if (existing != null)
            {
                await _repository.DeleteAsync(characterMap.Id, ct);
            }

            await _repository.AddAsync(characterMap, ct);
            importedCharacterMaps.Add(characterMap);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Imported {Count} character maps from YAML", importedCharacterMaps.Count);
        return importedCharacterMaps;
    }
}

