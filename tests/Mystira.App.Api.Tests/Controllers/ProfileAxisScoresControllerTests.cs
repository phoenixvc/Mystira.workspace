using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Mystira.App.Api.Controllers;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Api.Tests.Controllers;

public class ProfileAxisScoresControllerTests
{
    private readonly Mock<IPlayerScenarioScoreRepository> _scoreRepository;
    private readonly ProfileAxisScoresController _controller;

    public ProfileAxisScoresControllerTests()
    {
        _scoreRepository = new Mock<IPlayerScenarioScoreRepository>();
        _controller = new ProfileAxisScoresController(_scoreRepository.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task Get_WithExistingProfile_ReturnsScores()
    {
        var scores = new List<PlayerScenarioScore>
        {
            new()
            {
                ScenarioId = "scenario-1",
                GameSessionId = "session-1",
                CreatedAt = DateTime.UtcNow,
                AxisScores = new Dictionary<string, float>
                {
                    { "courage", 7.5f },
                    { "honesty", 5.0f }
                }
            }
        };

        _scoreRepository.Setup(r => r.GetByProfileIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(scores);

        var result = await _controller.Get("profile-1");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ProfileAxisScoresController.AxisScoresResponse>().Subject;
        response.ProfileId.Should().Be("profile-1");
        response.Items.Should().HaveCount(1);
        response.Items[0].ScenarioId.Should().Be("scenario-1");
        response.Items[0].AxisScores.Should().ContainKey("courage");
        response.Items[0].AxisScores["courage"].Should().Be(7.5f);
    }

    [Fact]
    public async Task Get_WithNoScores_ReturnsEmptyResponse()
    {
        _scoreRepository.Setup(r => r.GetByProfileIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerScenarioScore>());

        var result = await _controller.Get("profile-1");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ProfileAxisScoresController.AxisScoresResponse>().Subject;
        response.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_PreservesAxisScoreCaseInsensitivity()
    {
        var scores = new List<PlayerScenarioScore>
        {
            new()
            {
                ScenarioId = "s1",
                GameSessionId = "gs1",
                CreatedAt = DateTime.UtcNow,
                AxisScores = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Courage", 5.0f }
                }
            }
        };

        _scoreRepository.Setup(r => r.GetByProfileIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(scores);

        var result = await _controller.Get("profile-1");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ProfileAxisScoresController.AxisScoresResponse>().Subject;
        response.Items[0].AxisScores.Comparer.Should().Be(StringComparer.OrdinalIgnoreCase);
    }
}
