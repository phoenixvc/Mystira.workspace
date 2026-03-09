using Mystira.Application.Parsers;
using Mystira.Domain.Models;
using ContractsCreateScenarioRequest = Mystira.Contracts.App.Requests.Scenarios.CreateScenarioRequest;

namespace Mystira.Admin.Api.Models;

/// <summary>
/// Facade for creating CreateScenarioRequest from dictionary data
/// Delegates to specialized parsers in Application layer
/// </summary>
public static class ScenarioRequestCreator
{
    public static ContractsCreateScenarioRequest Create(Dictionary<object, object> scenarioData)
    {
        return ScenarioParser.Create(scenarioData);
    }

    public static Scene ParseSceneFromDictionary(IDictionary<object, object> sceneDict)
    {
        return SceneParser.Parse(sceneDict);
    }
}
