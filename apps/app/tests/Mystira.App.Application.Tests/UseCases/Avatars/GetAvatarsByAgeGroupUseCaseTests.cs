using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Application.Ports.Data;
using Mystira.App.Application.UseCases.Avatars;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.Avatars;

public class GetAvatarsByAgeGroupUseCaseTests
{
    private readonly Mock<IAvatarConfigurationFileRepository> _repository;
    private readonly Mock<ILogger<GetAvatarsByAgeGroupUseCase>> _logger;
    private readonly GetAvatarsByAgeGroupUseCase _useCase;

    public GetAvatarsByAgeGroupUseCaseTests()
    {
        _repository = new Mock<IAvatarConfigurationFileRepository>();
        _logger = new Mock<ILogger<GetAvatarsByAgeGroupUseCase>>();
        _useCase = new GetAvatarsByAgeGroupUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithMatchingAgeGroup_ReturnsAvatars()
    {
        var config = new AvatarConfigurationFile
        {
            AgeGroupAvatars = new Dictionary<string, List<string>>
            {
                { "6-9", new List<string> { "avatar-1", "avatar-2" } },
                { "10-12", new List<string> { "avatar-3" } }
            }
        };
        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        var result = await _useCase.ExecuteAsync("6-9");

        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyAgeGroup_ThrowsValidationException(string? ageGroup)
    {
        var act = () => _useCase.ExecuteAsync(ageGroup!);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
