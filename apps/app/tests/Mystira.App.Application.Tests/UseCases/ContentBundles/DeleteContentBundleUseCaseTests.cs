using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.ContentBundles;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.UseCases.ContentBundles;

public class DeleteContentBundleUseCaseTests
{
    private readonly Mock<IContentBundleRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<DeleteContentBundleUseCase>> _logger;
    private readonly DeleteContentBundleUseCase _useCase;

    public DeleteContentBundleUseCaseTests()
    {
        _repository = new Mock<IContentBundleRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<DeleteContentBundleUseCase>>();
        _useCase = new DeleteContentBundleUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingId_ReturnsTrue()
    {
        var bundle = new ContentBundle { Id = "b1" };
        _repository.Setup(r => r.GetByIdAsync("b1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);

        var result = await _useCase.ExecuteAsync("b1");

        result.Should().BeTrue();
        _repository.Verify(r => r.DeleteAsync("b1", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingId_ReturnsFalse()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(ContentBundle));

        var result = await _useCase.ExecuteAsync("missing");

        result.Should().BeFalse();
        _repository.Verify(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyId_ThrowsValidationException(string? bundleId)
    {
        var act = () => _useCase.ExecuteAsync(bundleId!);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
