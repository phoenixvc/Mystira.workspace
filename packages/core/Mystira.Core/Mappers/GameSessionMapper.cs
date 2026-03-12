using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Contracts.App.Models.GameSessions;
using Mystira.Contracts.App.Responses.GameSessions;
using ContractCharacterAssignmentDto = Mystira.Contracts.App.Models.CharacterAssignmentDto;

namespace Mystira.Core.Mappers;

/// <summary>
/// Centralizes mapping between GameSession domain model and contract DTOs.
/// Eliminates duplicated mapping logic across query handlers and command handlers.
/// </summary>
public static class GameSessionMapper
{
    /// <summary>
    /// Maps a domain GameSession to a contract GameSessionResponse.
    /// This is a pure mapping method - callers must ensure
    /// RecalculateCompassProgressFromHistory() has been called before invoking
    /// this method if up-to-date compass progress is needed.
    /// </summary>
    public static GameSessionResponse ToResponse(GameSession session)
    {
        return new GameSessionResponse
        {
            Id = session.Id,
            ScenarioId = session.ScenarioId,
            AccountId = session.AccountId,
            ProfileId = session.ProfileId,
            PlayerNames = session.PlayerNames,
            CharacterAssignments = MapCharacterAssignments(session.CharacterAssignments?.ToList()),
            PlayerCompassProgressTotals = session.PlayerCompassProgressTotals.Select(p => new PlayerCompassProgressDto
            {
                PlayerId = string.Empty,
                Axis = p.Key,
                Total = p.Value
            }).ToList(),
            Status = session.Status.ToString(),
            CurrentSceneId = session.CurrentSceneId ?? string.Empty,
            ChoiceCount = session.ChoiceHistory?.Count ?? 0,
            EchoCount = session.EchoHistory?.Count ?? 0,
            AchievementCount = session.Achievements?.Count ?? 0,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            ElapsedTime = session.GetTotalElapsedTime(),
            IsPaused = session.Status == SessionStatus.Paused,
            SceneCount = session.ChoiceHistory?.Select(c => c.SceneId).Distinct().Count() ?? 0,
            TargetAgeGroup = session.TargetAgeGroup ?? string.Empty
        };
    }

    /// <summary>
    /// Maps a list of domain GameSessions to contract GameSessionResponses.
    /// </summary>
    public static List<GameSessionResponse> ToResponseList(IEnumerable<GameSession> sessions)
    {
        return sessions.Select(ToResponse).ToList();
    }

    /// <summary>
    /// Maps domain CharacterAssignments to contract CharacterAssignmentDtos.
    /// </summary>
    public static List<CharacterAssignmentDto> MapCharacterAssignments(
        List<SessionCharacterAssignment>? assignments)
    {
        if (assignments == null || assignments.Count == 0)
            return new List<CharacterAssignmentDto>();

        return assignments.Select(ca => new CharacterAssignmentDto
        {
            CharacterId = ca.CharacterId,
            CharacterName = ca.CharacterName,
            Image = ca.Image,
            Audio = ca.Audio,
            Role = ca.Role,
            Archetype = ca.Archetype,
            IsUnused = ca.IsUnused,
            PlayerAssignment = ca.PlayerAssignment == null ? null : new PlayerAssignmentDto
            {
                Type = ca.PlayerAssignment.Type.ToString(),
                ProfileId = ca.PlayerAssignment.ProfileId,
                ProfileName = ca.PlayerAssignment.ProfileName,
                ProfileImage = ca.PlayerAssignment.ProfileImage,
                SelectedAvatarMediaId = ca.PlayerAssignment.SelectedAvatarMediaId,
                GuestName = ca.PlayerAssignment.GuestName,
                GuestAgeRange = ca.PlayerAssignment.GuestAgeRange,
                GuestAvatar = ca.PlayerAssignment.GuestAvatar,
                SaveAsProfile = ca.PlayerAssignment.SaveAsProfile
            }
        }).ToList();
    }

    /// <summary>
    /// Maps a contract CharacterAssignmentDto to a domain SessionCharacterAssignment.
    /// Used by command handlers to map incoming request DTOs to domain models.
    /// </summary>
    public static SessionCharacterAssignment ToDomain(ContractCharacterAssignmentDto dto)
    {
        return new SessionCharacterAssignment
        {
            CharacterId = dto.CharacterId,
            CharacterName = dto.CharacterName,
            Image = dto.Image,
            Audio = dto.Audio,
            Role = dto.Role,
            Archetype = dto.Archetype,
            PlayerAssignment = dto.PlayerAssignment == null
                ? null
                : new SessionPlayerAssignment
                {
                    Type = Enum.TryParse<PlayerType>(dto.PlayerAssignment.Type, out var pt) ? pt : PlayerType.Profile,
                    ProfileId = dto.PlayerAssignment.ProfileId,
                    ProfileName = dto.PlayerAssignment.ProfileName,
                    ProfileImage = dto.PlayerAssignment.ProfileImage,
                    SelectedAvatarMediaId = dto.PlayerAssignment.SelectedAvatarMediaId,
                    GuestName = dto.PlayerAssignment.GuestName,
                    GuestAgeRange = dto.PlayerAssignment.GuestAgeRange,
                    GuestAvatar = dto.PlayerAssignment.GuestAvatar,
                    SaveAsProfile = dto.PlayerAssignment.SaveAsProfile
                }
        };
    }
}
