using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.CompassAxes.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.CompassAxes;

public class GetAllCompassAxesQueryHandlerTests
{
    private readonly Mock<ICompassAxisRepository> _repository;
    private readonly Mock<ILogger<GetAllCompassAxesQuery>> _logger;

    public GetAllCompassAxesQueryHandlerTests()
    {
        _repository = new Mock<ICompassAxisRepository>();
        _logger = new Mock<ILogger<GetAllCompassAxesQuery>>();
    }

    [Fact]
    public async Task Handle_ReturnsAllCompassAxes()
    {
        // Arrange
        var axes = new List<CompassAxisDefinition>
        {
            new CompassAxisDefinition { Id = "courage", Name = "Courage", Description = "Bravery in the face of fear" },
            new CompassAxisDefinition { Id = "wisdom", Name = "Wisdom", Description = "Knowledge and good judgment" },
            new CompassAxisDefinition { Id = "kindness", Name = "Kindness", Description = "Compassion towards others" },
            new CompassAxisDefinition { Id = "honesty", Name = "Honesty", Description = "Truthfulness and integrity" }
        };

        var query = new GetAllCompassAxesQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(axes);

        // Act
        var result = await GetAllCompassAxesQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(4);
        result.Should().Contain(a => a.Id == "courage");
        result.Should().Contain(a => a.Id == "wisdom");
        result.Should().Contain(a => a.Id == "kindness");
        result.Should().Contain(a => a.Id == "honesty");
    }

    [Fact]
    public async Task Handle_WithNoAxes_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetAllCompassAxesQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CompassAxisDefinition>());

        // Act
        var result = await GetAllCompassAxesQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ReturnsAxesWithAllProperties()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-30);
        var updatedAt = DateTime.UtcNow.AddDays(-5);
        var axes = new List<CompassAxisDefinition>
        {
            new CompassAxisDefinition
            {
                Id = "courage",
                Name = "Courage",
                Description = "Bravery in the face of fear",
                IsDeleted = false,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            }
        };

        var query = new GetAllCompassAxesQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(axes);

        // Act
        var result = await GetAllCompassAxesQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var axis = result[0];
        axis.Id.Should().Be("courage");
        axis.Name.Should().Be("Courage");
        axis.Description.Should().Be("Bravery in the face of fear");
        axis.IsDeleted.Should().BeFalse();
        axis.CreatedAt.Should().Be(createdAt);
        axis.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public async Task Handle_ReturnsAllAxesFromRepository()
    {
        // Arrange
        // This test verifies the handler passes through all axes returned by the repository
        // Filtering of deleted axes is the repository's responsibility
        var axes = new List<CompassAxisDefinition>
        {
            new CompassAxisDefinition { Id = "courage", Name = "Courage", IsDeleted = false },
            new CompassAxisDefinition { Id = "wisdom", Name = "Wisdom", IsDeleted = false }
        };

        var query = new GetAllCompassAxesQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(axes);

        // Act
        var result = await GetAllCompassAxesQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(axes);
    }

    [Fact]
    public async Task Handle_CallsRepositoryOnce()
    {
        // Arrange
        var query = new GetAllCompassAxesQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CompassAxisDefinition>());

        // Act
        await GetAllCompassAxesQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _repository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PropagatesCancellationTokenToRepository()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var query = new GetAllCompassAxesQuery();

        _repository.Setup(r => r.GetAllAsync(cts.Token))
            .ReturnsAsync(new List<CompassAxisDefinition>());

        // Act
        await GetAllCompassAxesQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            cts.Token);

        // Assert - verify the exact token was passed, not just any token
        _repository.Verify(r => r.GetAllAsync(cts.Token), Times.Once);
    }
}
