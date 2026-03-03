namespace Mystira.Contracts.StoryGenerator.StoryConsistency;

/// <summary>
/// Represents a compressed path in the scenario graph corresponding to a path taken by the player. It gives
/// the story text corresponding to the path (comprised of a set of nodes, i.e., scene id's).
/// </summary>
public class ScenarioPath(IEnumerable<string> sceneIds, string story)
{
    /// <summary>
    /// The scene id's of the path.
    /// </summary>
    public string[] SceneIds { get; set; } = sceneIds.ToArray();

    /// <summary>
    /// The story text corresponding to the path.
    /// </summary>
    public string Story { get; set; } = story;
}
