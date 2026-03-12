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
    Task<AvatarConfigurationFile?> GetAsync(CancellationToken ct = default);

    /// <summary>
    /// Adds or updates the avatar configuration file.
    /// </summary>
    Task<AvatarConfigurationFile> AddOrUpdateAsync(AvatarConfigurationFile entity, CancellationToken ct = default);

    /// <summary>
    /// Deletes the avatar configuration file.
    /// </summary>
    Task DeleteAsync(CancellationToken ct = default);
}
