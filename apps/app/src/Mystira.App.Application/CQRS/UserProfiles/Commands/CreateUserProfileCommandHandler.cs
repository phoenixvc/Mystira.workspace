using Mystira.App.Application.UseCases.UserProfiles;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;

namespace Mystira.App.Application.CQRS.UserProfiles.Commands;

/// <summary>
/// Wolverine handler for CreateUserProfileCommand.
/// Performs handler-level name validation, then delegates to CreateUserProfileUseCase
/// which owns the full business logic including duplicate checks, theme validation,
/// age group validation, and entity creation.
/// </summary>
public static class CreateUserProfileCommandHandler
{
    /// <summary>
    /// Handles the CreateUserProfileCommand by delegating to the UseCase.
    /// Wolverine injects the UseCase as a method parameter.
    /// </summary>
    public static async Task<UserProfile> Handle(
        CreateUserProfileCommand command,
        CreateUserProfileUseCase createUserProfileUseCase,
        CancellationToken ct)
    {
        var request = command.Request;

        // Handler-level name validation (UseCase trusts DataAnnotations on the request)
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("name", "Profile name is required");
        }

        if (request.Name.Trim().Length < 2)
        {
            throw new ValidationException("name", "Profile name must be at least 2 characters long");
        }

        // Pre-fill ID if not provided (handler generates IDs for new profiles)
        if (string.IsNullOrEmpty(request.Id))
        {
            request.Id = Guid.NewGuid().ToString();
        }

        return await createUserProfileUseCase.ExecuteAsync(request, ct);
    }
}
