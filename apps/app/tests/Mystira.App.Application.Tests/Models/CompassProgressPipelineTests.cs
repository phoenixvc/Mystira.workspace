using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Mystira.Contracts.App.Requests.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.App.Infrastructure.Data;

namespace Mystira.App.Application.Tests.Models;

public class CompassProgressPipelineTests
{
    [Fact]
    public void GameSession_RecalculateCompassProgressFromHistory_ComputesAxisTotals()
    {
        var session = new GameSession
        {
            Id = "s1",
            AccountId = "a1",
            ProfileId = "owner",
            Status = SessionStatus.InProgress,
            StartTime = DateTime.UtcNow,
            CharacterAssignments = new List<SessionCharacterAssignment>
            {
                new()
                {
                    CharacterId = "c1",
                    CharacterName = "Hero",
                    PlayerAssignment = new SessionPlayerAssignment
                    {
                        Type = PlayerType.Profile,
                        ProfileId = "p1",
                        ProfileName = "Player One"
                    }
                }
            }
        };

        session.ChoiceHistory.AddRange([
            new SessionChoice
            {
                SceneId = "scene1",
                ChoiceText = "A",
                NextScene = "scene2",
                PlayerId = "p1",
                CompassChange = new CompassChange { AxisId = "honesty", Delta = 1 }
            },
            new SessionChoice
            {
                SceneId = "scene2",
                ChoiceText = "B",
                NextScene = "scene3",
                PlayerId = "p1",
                CompassChange = new CompassChange { AxisId = "honesty", Delta = -1 }
            },
            new SessionChoice
            {
                SceneId = "scene3",
                ChoiceText = "C",
                NextScene = "scene4",
                CompassChange = new CompassChange { AxisId = "honesty", Delta = 1 }
            },
            new SessionChoice
            {
                SceneId = "scene4",
                ChoiceText = "D",
                NextScene = "scene5",
                PlayerId = "guest:buddy",
                CompassChange = new CompassChange { AxisId = "kindness", Delta = 10 }
            }
        ]);

        session.RecalculateCompassProgressFromHistory();

        // New model aggregates per-axis, not per-player
        session.PlayerCompassProgressTotals.Should().ContainKey("honesty");
        session.PlayerCompassProgressTotals["honesty"].Should().Be(1); // 1 + (-1) + 1 = 1
        session.PlayerCompassProgressTotals.Should().ContainKey("kindness");
        session.PlayerCompassProgressTotals["kindness"].Should().Be(10);
    }

    [Fact]
    public async Task MystiraAppDbContext_Persists_ChoiceCompassFields()
    {
        var options = new DbContextOptionsBuilder<MystiraAppDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        await using var context = new MystiraAppDbContext(options);

        var session = new GameSession
        {
            Id = "s1",
            AccountId = "a1",
            ProfileId = "owner",
            Status = SessionStatus.InProgress,
            StartTime = DateTime.UtcNow,
            CharacterAssignments = new List<SessionCharacterAssignment>
            {
                new()
                {
                    CharacterId = "c1",
                    CharacterName = "Hero",
                    PlayerAssignment = new SessionPlayerAssignment
                    {
                        Type = PlayerType.Profile,
                        ProfileId = "p1",
                        ProfileName = "Player One"
                    }
                }
            }
        };

        session.ChoiceHistory.Add(new SessionChoice
        {
            SceneId = "scene1",
            ChoiceText = "A",
            NextScene = "scene2",
            PlayerId = "p1",
            CompassAxis = "honesty",
            CompassDirection = "positive",
            CompassDelta = 1.0,
            CompassChange = new CompassChange { AxisId = "honesty", Delta = 1 }
        });

        session.RecalculateCompassProgressFromHistory();

        context.GameSessions.Add(session);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var loaded = await context.GameSessions.SingleAsync(s => s.Id == "s1");

        loaded.ChoiceHistory.Should().HaveCount(1);
        loaded.ChoiceHistory[0].PlayerId.Should().Be("p1");
        loaded.ChoiceHistory[0].CompassAxis.Should().Be("honesty");
        loaded.ChoiceHistory[0].CompassDirection.Should().Be("positive");
        loaded.ChoiceHistory[0].CompassDelta.Should().Be(1.0);

        loaded.PlayerCompassProgressTotals.Should().ContainKey("honesty");
        loaded.PlayerCompassProgressTotals["honesty"].Should().Be(1);
    }

    [Fact]
    public void MakeChoiceRequest_SerializesAndDeserializes_PlayerAndCompassFields()
    {
        var req = new MakeChoiceRequest
        {
            SessionId = "s1",
            SceneId = "scene1",
            ChoiceText = "A",
            NextSceneId = "scene2",
            PlayerId = "p1",
            CompassAxis = "honesty",
            CompassDirection = "negative",
            CompassDelta = 0.5
        };

        var json = JsonSerializer.Serialize(req, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        json.Should().Contain("\"playerId\"");
        json.Should().Contain("\"compassAxis\"");

        var roundTrip = JsonSerializer.Deserialize<MakeChoiceRequest>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        roundTrip.Should().NotBeNull();
        roundTrip!.PlayerId.Should().Be("p1");
        roundTrip.CompassAxis.Should().Be("honesty");
        roundTrip.CompassDirection.Should().Be("negative");
        roundTrip.CompassDelta.Should().Be(0.5);
    }
}
