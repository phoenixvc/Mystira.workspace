using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.Avatars;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.UseCases.Avatars;

public class GetAvatarConfigurationsUseCaseTests
{
    private readonly Mock<IAvatarConfigurationFileRepository> _repository;
    private readonly Mock<ILogger<GetAvatarConfigurationsUseCase>> _logger;
    private readonly GetAvatarConfigurationsUseCase _useCase;

    public GetAvatarConfigurationsUseCaseTests()
    {
        _repository = new Mock<IAvatarConfigurationFileRepository>();
        _logger = new Mock<ILogger<GetAvatarConfigurationsUseCase>>();
        _useCase = new GetAvatarConfigurationsUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingConfig_ReturnsAvatarResponse()
    {
        var config = new AvatarConfigurationFile
        {
            Id = "config-1",
            AgeGroupAvatars = new Dictionary<string, List<string>>
            {
                { "6-9", new List<string> { "avatar-1" } }
            }
        };
        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        var result = await _useCase.ExecuteAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithNoConfig_ReturnsDefaultResponse()
    {
        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(AvatarConfigurationFile));

        var result = await _useCase.ExecuteAsync();

        result.Should().NotBeNull();
    }
}
