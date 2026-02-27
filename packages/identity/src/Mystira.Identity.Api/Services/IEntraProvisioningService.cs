namespace Mystira.Identity.Api.Services;

/// <summary>
/// Provides services for provisioning and managing users in Microsoft Entra ID.
/// Handles user creation, lookup, and linking operations for the Mystira platform.
/// </summary>
public interface IEntraProvisioningService
{
    /// <summary>
    /// Provisions a new user in Entra ID with the specified email and display name.
    /// </summary>
    /// <param name="email">The email address for the new user.</param>
    /// <param name="displayName">The display name for the new user.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A provisioning result indicating success or failure with relevant details.</returns>
    /// <exception cref="ArgumentException">Thrown when email or display name is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the service is not configured.</exception>
    Task<EntraProvisioningResult> ProvisionUserAsync(string email, string displayName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds an existing Entra ID user by their email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The Entra user if found; otherwise null.</returns>
    /// <exception cref="ArgumentException">Thrown when email is invalid.</exception>
    Task<EntraUser?> FindUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Links an existing account to an Entra ID user by storing the object ID reference.
    /// </summary>
    /// <param name="accountId">The local account ID to link.</param>
    /// <param name="entraObjectId">The Entra ID object ID to link with.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A provisioning result indicating success or failure with relevant details.</returns>
    /// <exception cref="ArgumentException">Thrown when account ID or Entra object ID is invalid.</exception>
    Task<EntraProvisioningResult> LinkExistingUserAsync(string accountId, string entraObjectId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of an Entra ID provisioning operation.
/// Contains success status, object ID, error information, and provisioning state.
/// </summary>
/// <param name="IsSuccess">True if the operation succeeded; false otherwise.</param>
/// <param name="EntraObjectId">The Entra ID object ID if successful; otherwise null.</param>
/// <param name="ErrorMessage">Error message describing the failure, if any.</param>
/// <param name="Status">The current status of the provisioning operation.</param>
public record EntraProvisioningResult(
    bool IsSuccess,
    string? EntraObjectId = null,
    string? ErrorMessage = null,
    ProvisioningStatus Status = ProvisioningStatus.Pending);

/// <summary>
/// Represents a user account in Microsoft Entra ID.
/// Contains essential user information for provisioning and linking operations.
/// </summary>
/// <param name="ObjectId">The unique Entra ID object identifier.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="DisplayName">The user's display name.</param>
/// <param name="IsEnabled">True if the user account is enabled; defaults to true.</param>
public record EntraUser(
    string ObjectId,
    string Email,
    string DisplayName,
    bool IsEnabled = true);

/// <summary>
/// Represents the state of an external provisioning operation.
/// Used to track the progress and status of user provisioning jobs.
/// </summary>
public enum ProvisioningStatus
{
    /// <summary>
    /// The provisioning operation is awaiting completion or retry.
    /// </summary>
    Pending,

    /// <summary>
    /// The provisioning operation completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// The provisioning operation encountered an error and failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The provisioning operation is scheduled to be retried after a failure.
    /// </summary>
    Retrying
}
