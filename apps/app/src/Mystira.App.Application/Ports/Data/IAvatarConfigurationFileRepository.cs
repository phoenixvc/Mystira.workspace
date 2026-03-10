using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Ports.Data;

/// <summary>
/// Repository interface for AvatarConfigurationFile singleton entity
/// </summary>
public interface IAvatarConfigurationFileRepository
{
    Task<AvatarConfigurationFile?> GetAsync(CancellationToken ct = default);
    Task<AvatarConfigurationFile> AddOrUpdateAsync(AvatarConfigurationFile entity, CancellationToken ct = default);
    Task DeleteAsync(CancellationToken ct = default);
}

