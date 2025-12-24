namespace Mystira.Shared.Authorization;

/// <summary>
/// Platform-wide permission constants.
/// These map to Entra ID app roles and custom claims.
/// </summary>
public static class Permissions
{
    // Scenario permissions
    public const string ScenariosRead = "scenarios.read";
    public const string ScenariosWrite = "scenarios.write";
    public const string ScenariosDelete = "scenarios.delete";
    public const string ScenariosPublish = "scenarios.publish";

    // Story permissions
    public const string StoriesRead = "stories.read";
    public const string StoriesWrite = "stories.write";
    public const string StoriesDelete = "stories.delete";
    public const string StoriesGenerate = "stories.generate";

    // Character permissions
    public const string CharactersRead = "characters.read";
    public const string CharactersWrite = "characters.write";
    public const string CharactersDelete = "characters.delete";

    // Publishing permissions
    public const string PublishRead = "publish.read";
    public const string PublishWrite = "publish.write";
    public const string PublishApprove = "publish.approve";

    // Admin permissions
    public const string AdminRead = "admin.read";
    public const string AdminWrite = "admin.write";
    public const string AdminUsers = "admin.users";
    public const string AdminSettings = "admin.settings";

    // API permissions
    public const string ApiAccess = "api.access";
    public const string ApiAdmin = "api.admin";
}

/// <summary>
/// Platform-wide role constants.
/// </summary>
public static class Roles
{
    /// <summary>
    /// Full platform administrator
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Content moderator
    /// </summary>
    public const string Moderator = "Moderator";

    /// <summary>
    /// Content creator/author
    /// </summary>
    public const string Creator = "Creator";

    /// <summary>
    /// Publisher/editor
    /// </summary>
    public const string Publisher = "Publisher";

    /// <summary>
    /// Standard user/reader
    /// </summary>
    public const string User = "User";

    /// <summary>
    /// Service account for service-to-service communication
    /// </summary>
    public const string Service = "Service";
}
