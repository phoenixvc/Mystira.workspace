using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.EchoTypes.Commands;
using Mystira.App.Application.CQRS.EchoTypes.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Services;
using Mystira.App.Domain.Models;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.EchoTypes;

public class EchoTypeHandlerTests
{
    private readonly Mock<IEchoTypeRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IQueryCacheInvalidationService> _cacheInvalidation;
    private readonly Mock<ILogger<CreateEchoTypeCommand>> _createLogger;
    private readonly Mock<ILogger<DeleteEchoTypeCommand>> _deleteLogger;
    private readonly Mock<ILogger<UpdateEchoTypeCommand>> _updateLogger;
    private readonly Mock<ILogger<GetAllEchoTypesQuery>> _getAllLogger;
    private readonly Mock<ILogger<GetEchoTypeByIdQuery>> _getByIdLogger;
    private readonly Mock<ILogger<ValidateEchoTypeQuery>> _validateLogger;

    public EchoTypeHandlerTests()
    {
        _repository = new Mock<IEchoTypeRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _cacheInvalidation = new Mock<IQueryCacheInvalidationService>();
        _createLogger = new Mock<ILogger<CreateEchoTypeCommand>>();
        _deleteLogger = new Mock<ILogger<DeleteEchoTypeCommand>>();
        _updateLogger = new Mock<ILogger<UpdateEchoTypeCommand>>();
        _getAllLogger = new Mock<ILogger<GetAllEchoTypesQuery>>();
        _getByIdLogger = new Mock<ILogger<GetEchoTypeByIdQuery>>();
        _validateLogger = new Mock<ILogger<ValidateEchoTypeQuery>>();
    }

    #region CreateEchoTypeCommandHandler Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedEchoType()
    {
        var command = new CreateEchoTypeCommand("Whisper", "A soft echo type", "emotional");

        var result = await CreateEchoTypeCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _createLogger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Whisper");
        result.Description.Should().Be("A soft echo type");
        result.Id.Should().NotBeNullOrEmpty();
        _repository.Verify(r => r.AddAsync(It.IsAny<EchoTypeDefinition>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheInvalidation.Verify(c => c.InvalidateCacheByPrefix("MasterData:EchoTypes"), Times.Once);
    }

    [Fact]
    public async Task Create_WithEmptyName_ThrowsArgumentException()
    {
        var command = new CreateEchoTypeCommand("", "Description", "moral");

        var act = () => CreateEchoTypeCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _createLogger.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region DeleteEchoTypeCommandHandler Tests

    [Fact]
    public async Task Delete_WithExistingId_ReturnsTrue()
    {
        var echoType = new EchoTypeDefinition { Id = "echo-1", Name = "Whisper" };
        _repository.Setup(r => r.GetByIdAsync("echo-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(echoType);

        var result = await DeleteEchoTypeCommandHandler.Handle(
            new DeleteEchoTypeCommand("echo-1"), _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _deleteLogger.Object, CancellationToken.None);

        result.Should().BeTrue();
        _repository.Verify(r => r.DeleteAsync("echo-1", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ReturnsFalse()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(EchoTypeDefinition));

        var result = await DeleteEchoTypeCommandHandler.Handle(
            new DeleteEchoTypeCommand("missing"), _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _deleteLogger.Object, CancellationToken.None);

        result.Should().BeFalse();
        _repository.Verify(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region UpdateEchoTypeCommandHandler Tests

    [Fact]
    public async Task Update_WithExistingId_ReturnsUpdatedEchoType()
    {
        var existing = new EchoTypeDefinition { Id = "echo-1", Name = "Old", Description = "Old desc" };
        _repository.Setup(r => r.GetByIdAsync("echo-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await UpdateEchoTypeCommandHandler.Handle(
            new UpdateEchoTypeCommand("echo-1", "Updated", "New desc", "emotional"), _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _updateLogger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated");
        result.Description.Should().Be("New desc");
        _repository.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithNonExistingId_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(EchoTypeDefinition));

        var result = await UpdateEchoTypeCommandHandler.Handle(
            new UpdateEchoTypeCommand("missing", "Name", "Desc", "moral"), _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _updateLogger.Object, CancellationToken.None);

        result.Should().BeNull();
    }

    #endregion

    #region GetAllEchoTypesQueryHandler Tests

    [Fact]
    public async Task GetAll_ReturnsAllEchoTypes()
    {
        var echoTypes = new List<EchoTypeDefinition>
        {
            new() { Id = "1", Name = "Whisper" },
            new() { Id = "2", Name = "Roar" }
        };
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(echoTypes);

        var result = await GetAllEchoTypesQueryHandler.Handle(
            new GetAllEchoTypesQuery(), _repository.Object, _getAllLogger.Object, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    #endregion

    #region GetEchoTypeByIdQueryHandler Tests

    [Fact]
    public async Task GetById_WithExistingId_ReturnsEchoType()
    {
        var echoType = new EchoTypeDefinition { Id = "echo-1", Name = "Whisper" };
        _repository.Setup(r => r.GetByIdAsync("echo-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(echoType);

        var result = await GetEchoTypeByIdQueryHandler.Handle(
            new GetEchoTypeByIdQuery("echo-1"), _repository.Object, _getByIdLogger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Whisper");
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(EchoTypeDefinition));

        var result = await GetEchoTypeByIdQueryHandler.Handle(
            new GetEchoTypeByIdQuery("missing"), _repository.Object, _getByIdLogger.Object, CancellationToken.None);

        result.Should().BeNull();
    }

    #endregion

    #region ValidateEchoTypeQueryHandler Tests

    [Fact]
    public async Task Validate_WithExistingName_ReturnsTrue()
    {
        _repository.Setup(r => r.ExistsByNameAsync("Whisper", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await ValidateEchoTypeQueryHandler.Handle(
            new ValidateEchoTypeQuery("Whisper"), _repository.Object, _validateLogger.Object, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithNonExistingName_ReturnsFalse()
    {
        _repository.Setup(r => r.ExistsByNameAsync("Unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await ValidateEchoTypeQueryHandler.Handle(
            new ValidateEchoTypeQuery("Unknown"), _repository.Object, _validateLogger.Object, CancellationToken.None);

        result.Should().BeFalse();
    }

    #endregion

    #region EchoTypeCategories Tests

    [Theory]
    [InlineData("moral", true)]
    [InlineData("emotional", true)]
    [InlineData("behavioral", true)]
    [InlineData("social", true)]
    [InlineData("cognitive", true)]
    [InlineData("meta", true)]
    [InlineData("MORAL", true)]
    [InlineData("Emotional", true)]
    [InlineData("invalid", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("physical", false)]
    public void EchoTypeCategories_IsValid_ReturnsExpected(string? category, bool expected)
    {
        EchoTypeCategories.IsValid(category).Should().Be(expected);
    }

    #endregion

    #region Category Validation Tests

    [Fact]
    public async Task Create_WithInvalidCategory_ThrowsArgumentException()
    {
        var command = new CreateEchoTypeCommand("Echo", "Description", "invalid_category");

        var act = () => CreateEchoTypeCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _createLogger.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("moral")]
    [InlineData("emotional")]
    [InlineData("behavioral")]
    [InlineData("social")]
    [InlineData("cognitive")]
    [InlineData("meta")]
    public async Task Create_WithValidCategory_Succeeds(string category)
    {
        var command = new CreateEchoTypeCommand("Test Echo", "Description", category);

        var result = await CreateEchoTypeCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _createLogger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result.Category.Should().Be(category);
    }

    [Fact]
    public async Task Update_WithInvalidCategory_ThrowsArgumentException()
    {
        var existing = new EchoTypeDefinition { Id = "echo-1", Name = "Echo", Category = "moral" };
        _repository.Setup(r => r.GetByIdAsync("echo-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var act = () => UpdateEchoTypeCommandHandler.Handle(
            new UpdateEchoTypeCommand("echo-1", "Echo", "Desc", "invalid_category"),
            _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _updateLogger.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region Cache Key Tests

    [Fact]
    public void GetAllEchoTypesQuery_HasCorrectCacheKey()
    {
        var query = new GetAllEchoTypesQuery();
        query.CacheKey.Should().Be("MasterData:EchoTypes:All");
    }

    [Fact]
    public void GetEchoTypeByIdQuery_HasCorrectCacheKey()
    {
        var query = new GetEchoTypeByIdQuery("test-id");
        query.CacheKey.Should().Be("MasterData:EchoTypes:test-id");
    }

    #endregion

    #region Timestamp Tests

    [Fact]
    public async Task Create_SetsTimestamps()
    {
        var before = DateTime.UtcNow;
        var command = new CreateEchoTypeCommand("Echo", "Desc", "moral");

        var result = await CreateEchoTypeCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _createLogger.Object, CancellationToken.None);

        result.CreatedAt.Should().BeOnOrAfter(before);
        result.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public async Task Update_SetsUpdatedTimestamp()
    {
        var existing = new EchoTypeDefinition
        {
            Id = "echo-1", Name = "Old", Category = "moral",
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _repository.Setup(r => r.GetByIdAsync("echo-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var before = DateTime.UtcNow;

        var result = await UpdateEchoTypeCommandHandler.Handle(
            new UpdateEchoTypeCommand("echo-1", "New", "Desc", "emotional"),
            _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _updateLogger.Object, CancellationToken.None);

        result!.UpdatedAt.Should().BeOnOrAfter(before);
    }

    #endregion

    #region Empty Collection Tests

    [Fact]
    public async Task GetAll_WhenEmpty_ReturnsEmptyList()
    {
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EchoTypeDefinition>());

        var result = await GetAllEchoTypesQueryHandler.Handle(
            new GetAllEchoTypesQuery(), _repository.Object, _getAllLogger.Object, CancellationToken.None);

        result.Should().BeEmpty();
    }

    #endregion
}
