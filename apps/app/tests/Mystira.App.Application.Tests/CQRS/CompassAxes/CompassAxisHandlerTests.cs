using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.CompassAxes.Commands;
using Mystira.App.Application.CQRS.CompassAxes.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Services;
using Mystira.App.Domain.Models;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.CompassAxes;

public class CompassAxisHandlerTests
{
    private readonly Mock<ICompassAxisRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IQueryCacheInvalidationService> _cacheInvalidation;
    private readonly Mock<ILogger<CreateCompassAxisCommand>> _createLogger;
    private readonly Mock<ILogger<DeleteCompassAxisCommand>> _deleteLogger;
    private readonly Mock<ILogger<UpdateCompassAxisCommand>> _updateLogger;
    private readonly Mock<ILogger<GetCompassAxisByIdQuery>> _getByIdLogger;
    private readonly Mock<ILogger<ValidateCompassAxisQuery>> _validateLogger;

    public CompassAxisHandlerTests()
    {
        _repository = new Mock<ICompassAxisRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _cacheInvalidation = new Mock<IQueryCacheInvalidationService>();
        _createLogger = new Mock<ILogger<CreateCompassAxisCommand>>();
        _deleteLogger = new Mock<ILogger<DeleteCompassAxisCommand>>();
        _updateLogger = new Mock<ILogger<UpdateCompassAxisCommand>>();
        _getByIdLogger = new Mock<ILogger<GetCompassAxisByIdQuery>>();
        _validateLogger = new Mock<ILogger<ValidateCompassAxisQuery>>();
    }

    #region CreateCompassAxisCommandHandler Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedCompassAxis()
    {
        var command = new CreateCompassAxisCommand("Courage", "Measures bravery and boldness");

        var result = await CreateCompassAxisCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _createLogger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Courage");
        result.Description.Should().Be("Measures bravery and boldness");
        result.Id.Should().NotBeNullOrEmpty();
        _repository.Verify(r => r.AddAsync(It.IsAny<CompassAxis>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheInvalidation.Verify(c => c.InvalidateCacheByPrefixAsync("MasterData:CompassAxes"), Times.Once);
    }

    [Fact]
    public async Task Create_WithEmptyName_ThrowsValidationException()
    {
        var command = new CreateCompassAxisCommand("", "Description");

        var act = () => CreateCompassAxisCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _createLogger.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region DeleteCompassAxisCommandHandler Tests

    [Fact]
    public async Task Delete_WithExistingId_ReturnsTrue()
    {
        var axis = new CompassAxis { Id = "axis-1", Name = "Courage" };
        _repository.Setup(r => r.GetByIdAsync("axis-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(axis);

        var result = await DeleteCompassAxisCommandHandler.Handle(
            new DeleteCompassAxisCommand("axis-1"), _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _deleteLogger.Object, CancellationToken.None);

        result.Should().BeTrue();
        _repository.Verify(r => r.DeleteAsync("axis-1", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheInvalidation.Verify(c => c.InvalidateCacheByPrefixAsync("MasterData:CompassAxes"), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ReturnsFalse()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(CompassAxis));

        var result = await DeleteCompassAxisCommandHandler.Handle(
            new DeleteCompassAxisCommand("missing"), _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _deleteLogger.Object, CancellationToken.None);

        result.Should().BeFalse();
        _repository.Verify(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region UpdateCompassAxisCommandHandler Tests

    [Fact]
    public async Task Update_WithExistingId_ReturnsUpdatedCompassAxis()
    {
        var existing = new CompassAxis { Id = "axis-1", Name = "Old", Description = "Old desc" };
        _repository.Setup(r => r.GetByIdAsync("axis-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await UpdateCompassAxisCommandHandler.Handle(
            new UpdateCompassAxisCommand("axis-1", "Updated", "New desc"), _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _updateLogger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated");
        result.Description.Should().Be("New desc");
        _repository.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheInvalidation.Verify(c => c.InvalidateCacheByPrefixAsync("MasterData:CompassAxes"), Times.Once);
    }

    [Fact]
    public async Task Update_WithNonExistingId_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(CompassAxis));

        var result = await UpdateCompassAxisCommandHandler.Handle(
            new UpdateCompassAxisCommand("missing", "Name", "Desc"), _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _updateLogger.Object, CancellationToken.None);

        result.Should().BeNull();
        _repository.Verify(r => r.UpdateAsync(It.IsAny<CompassAxis>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region GetCompassAxisByIdQueryHandler Tests

    [Fact]
    public async Task GetById_WithExistingId_ReturnsCompassAxis()
    {
        var axis = new CompassAxis { Id = "axis-1", Name = "Courage" };
        _repository.Setup(r => r.GetByIdAsync("axis-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(axis);

        var result = await GetCompassAxisByIdQueryHandler.Handle(
            new GetCompassAxisByIdQuery("axis-1"), _repository.Object, _getByIdLogger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Courage");
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(CompassAxis));

        var result = await GetCompassAxisByIdQueryHandler.Handle(
            new GetCompassAxisByIdQuery("missing"), _repository.Object, _getByIdLogger.Object, CancellationToken.None);

        result.Should().BeNull();
    }

    #endregion

    #region ValidateCompassAxisQueryHandler Tests

    [Fact]
    public async Task Validate_WithExistingName_ReturnsTrue()
    {
        _repository.Setup(r => r.ExistsByNameAsync("Courage", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await ValidateCompassAxisQueryHandler.Handle(
            new ValidateCompassAxisQuery("Courage"), _repository.Object, _validateLogger.Object, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithNonExistingName_ReturnsFalse()
    {
        _repository.Setup(r => r.ExistsByNameAsync("Unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await ValidateCompassAxisQueryHandler.Handle(
            new ValidateCompassAxisQuery("Unknown"), _repository.Object, _validateLogger.Object, CancellationToken.None);

        result.Should().BeFalse();
    }

    #endregion
}
