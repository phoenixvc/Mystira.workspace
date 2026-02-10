using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.FantasyThemes.Commands;
using Mystira.App.Application.CQRS.FantasyThemes.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Services;
using Mystira.App.Domain.Models;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.FantasyThemes;

public class FantasyThemeHandlerTests
{
    private readonly Mock<IFantasyThemeRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IQueryCacheInvalidationService> _cacheInvalidation;
    private readonly Mock<ILogger> _logger;

    public FantasyThemeHandlerTests()
    {
        _repository = new Mock<IFantasyThemeRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _cacheInvalidation = new Mock<IQueryCacheInvalidationService>();
        _logger = new Mock<ILogger>();
    }

    #region CreateFantasyThemeCommandHandler Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedTheme()
    {
        var command = new CreateFantasyThemeCommand("Medieval", "A medieval fantasy theme");

        var result = await CreateFantasyThemeCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Medieval");
        result.Description.Should().Be("A medieval fantasy theme");
        result.Id.Should().NotBeNullOrEmpty();
        _repository.Verify(r => r.AddAsync(It.IsAny<FantasyThemeDefinition>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheInvalidation.Verify(c => c.InvalidateCacheByPrefix("MasterData:FantasyThemes"), Times.Once);
    }

    [Fact]
    public async Task Create_WithEmptyName_ThrowsArgumentException()
    {
        var command = new CreateFantasyThemeCommand("", "Description");

        var act = () => CreateFantasyThemeCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region DeleteFantasyThemeCommandHandler Tests

    [Fact]
    public async Task Delete_WithExistingId_ReturnsTrue()
    {
        var theme = new FantasyThemeDefinition { Id = "theme-1", Name = "Medieval" };
        _repository.Setup(r => r.GetByIdAsync("theme-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(theme);

        var result = await DeleteFantasyThemeCommandHandler.Handle(
            new DeleteFantasyThemeCommand("theme-1"), _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
        _repository.Verify(r => r.DeleteAsync("theme-1", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ReturnsFalse()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(FantasyThemeDefinition));

        var result = await DeleteFantasyThemeCommandHandler.Handle(
            new DeleteFantasyThemeCommand("missing"), _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
    }

    #endregion

    #region UpdateFantasyThemeCommandHandler Tests

    [Fact]
    public async Task Update_WithExistingId_ReturnsUpdatedTheme()
    {
        var existing = new FantasyThemeDefinition { Id = "theme-1", Name = "Old", Description = "Old desc" };
        _repository.Setup(r => r.GetByIdAsync("theme-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await UpdateFantasyThemeCommandHandler.Handle(
            new UpdateFantasyThemeCommand("theme-1", "Sci-Fi", "A science fiction theme"), _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Sci-Fi");
        result.Description.Should().Be("A science fiction theme");
        _repository.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithNonExistingId_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(FantasyThemeDefinition));

        var result = await UpdateFantasyThemeCommandHandler.Handle(
            new UpdateFantasyThemeCommand("missing", "Name", "Desc"), _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.Should().BeNull();
    }

    #endregion

    #region GetAllFantasyThemesQueryHandler Tests

    [Fact]
    public async Task GetAll_ReturnsAllThemes()
    {
        var themes = new List<FantasyThemeDefinition>
        {
            new() { Id = "1", Name = "Medieval" },
            new() { Id = "2", Name = "Sci-Fi" }
        };
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(themes);

        var result = await GetAllFantasyThemesQueryHandler.Handle(
            new GetAllFantasyThemesQuery(), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    #endregion

    #region GetFantasyThemeByIdQueryHandler Tests

    [Fact]
    public async Task GetById_WithExistingId_ReturnsTheme()
    {
        var theme = new FantasyThemeDefinition { Id = "theme-1", Name = "Medieval" };
        _repository.Setup(r => r.GetByIdAsync("theme-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(theme);

        var result = await GetFantasyThemeByIdQueryHandler.Handle(
            new GetFantasyThemeByIdQuery("theme-1"), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Medieval");
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(FantasyThemeDefinition));

        var result = await GetFantasyThemeByIdQueryHandler.Handle(
            new GetFantasyThemeByIdQuery("missing"), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeNull();
    }

    #endregion

    #region ValidateFantasyThemeQueryHandler Tests

    [Fact]
    public async Task Validate_WithExistingName_ReturnsTrue()
    {
        _repository.Setup(r => r.ExistsByNameAsync("Medieval", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await ValidateFantasyThemeQueryHandler.Handle(
            new ValidateFantasyThemeQuery("Medieval"), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithNonExistingName_ReturnsFalse()
    {
        _repository.Setup(r => r.ExistsByNameAsync("Unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await ValidateFantasyThemeQueryHandler.Handle(
            new ValidateFantasyThemeQuery("Unknown"), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
    }

    #endregion
}
