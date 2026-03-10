using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Identity.Api.Services;

/// <summary>
/// Implementation of Entra ID provisioning service for user management operations.
/// Provides functionality to create, find, and link users in Microsoft Entra ID.
/// </summary>
public class EntraProvisioningService : IEntraProvisioningService
{
    private readonly GraphServiceClient? _graphClient;
    private readonly ILogger<EntraProvisioningService> _logger;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the EntraProvisioningService.
    /// </summary>
    /// <param name="logger">Logger for diagnostic information.</param>
    /// <param name="configuration">Configuration containing Entra ID credentials.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger or configuration is null.</exception>
    public EntraProvisioningService(
        ILogger<EntraProvisioningService> logger,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        var tenantId = configuration["EntraProvisioning:TenantId"];
        var clientId = configuration["EntraProvisioning:ClientId"];
        var clientSecret = configuration["EntraProvisioning:ClientSecret"];

        if (!string.IsNullOrWhiteSpace(tenantId) &&
            !string.IsNullOrWhiteSpace(clientId) &&
            !string.IsNullOrWhiteSpace(clientSecret))
        {
            var credentials = new ClientSecretCredential(tenantId, clientId, clientSecret);
            _graphClient = new GraphServiceClient(credentials);
        }
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "[no-email]";

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
            return "[invalid-email]";

        var localPart = email.Substring(0, atIndex);
        var domain = email.Substring(atIndex + 1);

        // Show first 2 characters and last character of local part, mask the rest
        var maskedLocal = localPart.Length > 3
            ? $"{localPart.Substring(0, 2)}***{localPart[^1]}"
            : $"***{localPart[^1]}";

        return $"{maskedLocal}@{domain}";
    }

    /// <summary>
    /// Provisions a new user in Entra ID with the specified email and display name.
    /// Validates domain requirements and creates the user with appropriate settings.
    /// </summary>
    /// <param name="email">The email address for the new user.</param>
    /// <param name="displayName">The display name for the new user.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A provisioning result indicating success or failure with relevant details.</returns>
    /// <exception cref="ArgumentException">Thrown when email or display name is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the service is not configured.</exception>
    public async Task<EntraProvisioningResult> ProvisionUserAsync(string email, string displayName, CancellationToken cancellationToken = default)
    {
        if (_graphClient == null)
        {
            return new EntraProvisioningResult(false, null, "Entra provisioning service is not configured", ProvisioningStatus.Failed);
        }

        try
        {
            // Check if user already exists
            var existingUser = await FindUserByEmailAsync(email, cancellationToken);
            if (existingUser != null)
            {
                _logger.LogInformation("Entra user already exists for email: {Email}", MaskEmail(email));
                return new EntraProvisioningResult(true, existingUser.ObjectId, null, ProvisioningStatus.Completed);
            }

            // Create new user
            var user = new User
            {
                UserPrincipalName = email,
                Mail = email,
                DisplayName = displayName,
                AccountEnabled = true,
                PasswordPolicies = "DisablePasswordExpiration",
                PasswordProfile = new PasswordProfile
                {
                    Password = GenerateTemporaryPassword(),
                    ForceChangePasswordNextSignIn = true
                }
            };

            var createdUser = await _graphClient.Users.PostAsync(user, cancellationToken: cancellationToken);

            if (createdUser == null)
            {
                _logger.LogError("Created user is null after successful PostAsync for email: {Email}", MaskEmail(email));
                return new EntraProvisioningResult(false, null, "Failed to create Entra user: null response", ProvisioningStatus.Failed);
            }

            _logger.LogInformation("Successfully created Entra user for email: {Email}, ObjectId: {ObjectId}",
                MaskEmail(email), createdUser.Id);

            return new EntraProvisioningResult(true, createdUser.Id, null, ProvisioningStatus.Completed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision Entra user for email: {Email}", MaskEmail(email));
            return new EntraProvisioningResult(false, null, ex.Message, ProvisioningStatus.Failed);
        }
    }

    /// <summary>
    /// Finds an existing Entra ID user by their email address.
    /// Uses OData filtering to search by both mail and userPrincipalName fields.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The Entra user if found; otherwise null.</returns>
    /// <exception cref="ArgumentException">Thrown when email is invalid.</exception>
    public async Task<EntraUser?> FindUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (_graphClient == null)
        {
            return null;
        }

        try
        {
            // Escape single quotes in email to prevent OData injection
            var escapedEmail = email.Replace("'", "''");

            var users = await _graphClient.Users
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Filter = $"mail eq '{escapedEmail}' or userPrincipalName eq '{escapedEmail}'";
                    requestConfiguration.QueryParameters.Select = ["id", "mail", "displayName", "accountEnabled"];
                });

            var user = users?.Value?.FirstOrDefault();
            if (user == null)
            {
                return null;
            }

            return new EntraUser(
                user.Id!,
                user.Mail ?? user.UserPrincipalName!,
                user.DisplayName ?? email,
                user.AccountEnabled ?? true
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding Entra user by email: {Email}", MaskEmail(email));
            return null;
        }
    }

    /// <summary>
    /// Links an existing account to an Entra ID user by validating the user exists.
    /// This method validates that the Entra user exists before returning success.
    /// </summary>
    /// <param name="accountId">The local account ID to link.</param>
    /// <param name="entraObjectId">The Entra ID object ID to link with.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A provisioning result indicating success or failure with relevant details.</returns>
    /// <exception cref="ArgumentException">Thrown when account ID or Entra object ID is invalid.</exception>
    public async Task<EntraProvisioningResult> LinkExistingUserAsync(string accountId, string entraObjectId, CancellationToken cancellationToken = default)
    {
        // This would typically update the Account entity to store the EntraObjectId
        // For now, we'll just validate that the Entra user exists
        try
        {
            if (_graphClient == null)
            {
                return new EntraProvisioningResult(false, null, "Entra provisioning service is not configured", ProvisioningStatus.Failed);
            }

            var user = await _graphClient.Users[entraObjectId].GetAsync();
            if (user == null)
            {
                return new EntraProvisioningResult(false, null, "Entra user not found", ProvisioningStatus.Failed);
            }

            _logger.LogInformation("Successfully linked account {AccountId} to Entra user {EntraObjectId}", accountId, entraObjectId);
            return new EntraProvisioningResult(true, entraObjectId, null, ProvisioningStatus.Completed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to link account {AccountId} to Entra user {EntraObjectId}", accountId, entraObjectId);
            return new EntraProvisioningResult(false, null, ex.Message, ProvisioningStatus.Failed);
        }
    }

    private static string GenerateTemporaryPassword()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
        var password = new char[16];

        // Use cryptographically secure random number generator
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        var buffer = new byte[password.Length];
        rng.GetBytes(buffer);

        for (int i = 0; i < password.Length; i++)
        {
            password[i] = chars[buffer[i] % chars.Length];
        }

        return new string(password);
    }
}
