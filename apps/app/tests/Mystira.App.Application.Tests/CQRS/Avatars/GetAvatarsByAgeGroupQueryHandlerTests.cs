using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.CQRS.Avatars.Queries;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.Avatars;

public class GetAvatarsByAgeGroupQueryHandlerTests
{
    private readonly Mock<IAvatarConfigurationFileRepository> _repository;
    private readonly Mock<ILogger<GetAvatarsByAgeGroupQuery>> _logger;

    public GetAvatarsByAgeGroupQueryHandlerTests()
    {
        _repository = new Mock<IAvatarConfigurationFileRepository>();
        _logger = new Mock<ILogger<GetAvatarsByAgeGroupQuery>>();
    }

    [Fact]
    public async Task Handle_WithExistingAgeGroup_ReturnsAvatars()
    {
        var configFile = new AvatarConfigurationFile
        {
            AgeGroupAvatars = new Dictionary<string, List<string>>
            {
                ["6-9"] = new List<string> { "avatar-1", "avatar-2", "avatar-3" }
            }
        };
        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(configFile);

        var result = await GetAvatarsByAgeGroupQueryHandler.Handle(
            new GetAvatarsByAgeGroupQuery("6-9"), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.AgeGroup.Should().Be("6-9");
        result.AvatarMediaIds.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithNonExistingAgeGroup_ReturnsEmptyList()
    {
        var configFile = new AvatarConfigurationFile
        {
            AgeGroupAvatars = new Dictionary<string, List<string>>()
        };
        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(configFile);

        var result = await GetAvatarsByAgeGroupQueryHandler.Handle(
            new GetAvatarsByAgeGroupQuery("99-100"), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.AvatarMediaIds.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithEmptyAgeGroup_ReturnsNull()
    {
        var result = await GetAvatarsByAgeGroupQueryHandler.Handle(
            new GetAvatarsByAgeGroupQuery(""), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeNull();
    }
}
