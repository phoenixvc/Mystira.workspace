using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository interface for AvatarConfigurationFile singleton entity
/// </summary>
public interface IAvatarConfigurationFileRepository
{
    /// <summary>
    /// Gets the avatar configuration file.
    /// </summary>
    /// <returns>The avatar configuration file if found; otherwise, null.</returns>
    Task<AvatarConfigurationFile?> GetAsync();

    /// <summary>
    /// Adds or updates the avatar configuration file.
    /// </summary>
    /// <param name="entity">The avatar configuration file to add or update.</param>
    /// <returns>The added or updated avatar configuration file.</returns>
    Task<AvatarConfigurationFile> AddOrUpdateAsync(AvatarConfigurationFile entity);

    /// <summary>
    /// Deletes the avatar configuration file.
    /// </summary>
    Task DeleteAsync();
}

