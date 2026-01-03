using Mystira.Domain.Models;
using Mystira.Contracts.App.Requests.CharacterMaps;

namespace Mystira.Admin.Api.Services;

public interface ICharacterMapApiService
{
    Task<List<CharacterMap>> GetAllCharacterMapsAsync();
    Task<CharacterMap?> GetCharacterMapAsync(string id);
    Task<CharacterMap> CreateCharacterMapAsync(CreateCharacterMapRequest request);
    Task<CharacterMap?> UpdateCharacterMapAsync(string id, UpdateCharacterMapRequest request);
    Task<bool> DeleteCharacterMapAsync(string id);
    Task<string> ExportCharacterMapsAsYamlAsync();
    Task<List<CharacterMap>> ImportCharacterMapsFromYamlAsync(Stream yamlStream);
}
