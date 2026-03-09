namespace Mystira.Shared.Authorization;

/// <summary>
/// Platform-wide permission constants.
/// These map to Entra ID app roles and custom claims.
/// </summary>
public static class Permissions
{
    /// <summary>Permission to read scenarios.</summary>
    public const string ScenariosRead = "scenarios.read";

    /// <summary>Permission to create or update scenarios.</summary>
    public const string ScenariosWrite = "scenarios.write";

    /// <summary>Permission to delete scenarios.</summary>
    public const string ScenariosDelete = "scenarios.delete";

    /// <summary>Permission to publish scenarios.</summary>
    public const string ScenariosPublish = "scenarios.publish";

    /// <summary>Permission to read stories.</summary>
    public const string StoriesRead = "stories.read";

    /// <summary>Permission to create or update stories.</summary>
    public const string StoriesWrite = "stories.write";

    /// <summary>Permission to delete stories.</summary>
    public const string StoriesDelete = "stories.delete";

    /// <summary>Permission to generate stories using AI.</summary>
    public const string StoriesGenerate = "stories.generate";

    /// <summary>Permission to read characters.</summary>
    public const string CharactersRead = "characters.read";

    /// <summary>Permission to create or update characters.</summary>
    public const string CharactersWrite = "characters.write";

    /// <summary>Permission to delete characters.</summary>
    public const string CharactersDelete = "characters.delete";

    /// <summary>Permission to read published content.</summary>
    public const string PublishRead = "publish.read";

    /// <summary>Permission to submit content for publishing.</summary>
    public const string PublishWrite = "publish.write";

    /// <summary>Permission to approve content for publishing.</summary>
    public const string PublishApprove = "publish.approve";

    /// <summary>Permission to read admin data.</summary>
    public const string AdminRead = "admin.read";

    /// <summary>Permission to write admin data.</summary>
    public const string AdminWrite = "admin.write";

    /// <summary>Permission to manage users.</summary>
    public const string AdminUsers = "admin.users";

    /// <summary>Permission to manage system settings.</summary>
    public const string AdminSettings = "admin.settings";

    /// <summary>Permission to access the API.</summary>
    public const string ApiAccess = "api.access";

    /// <summary>Permission for API administration.</summary>
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
