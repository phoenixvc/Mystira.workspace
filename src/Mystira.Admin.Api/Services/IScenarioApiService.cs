using Mystira.Domain.Models;
using Mystira.Contracts.App.Requests.Scenarios;
using Mystira.Contracts.App.Responses.Scenarios;

namespace Mystira.Admin.Api.Services;

public interface IScenarioApiService
{
    Task<ScenarioListResponse> GetScenariosAsync(ScenarioQueryRequest request);
    Task<Scenario?> GetScenarioByIdAsync(string id);
    Task<Scenario> CreateScenarioAsync(CreateScenarioRequest request);
    Task<Scenario?> UpdateScenarioAsync(string id, CreateScenarioRequest request);
    Task<bool> DeleteScenarioAsync(string id);
    Task<List<Scenario>> GetScenariosByAgeGroupAsync(string ageGroup);
    Task<List<Scenario>> GetFeaturedScenariosAsync();
    Task ValidateScenarioAsync(Scenario scenario);
    Task<ScenarioReferenceValidation> ValidateScenarioReferencesAsync(string scenarioId, bool includeMetadataValidation = true);
    Task<List<ScenarioReferenceValidation>> ValidateAllScenarioReferencesAsync(bool includeMetadataValidation = true);
}
