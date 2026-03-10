using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.ContentBundles.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.ContentBundles;

public class GetContentBundlesByAgeGroupQueryHandlerTests
{
    private readonly Mock<IContentBundleRepository> _repository;
    private readonly Mock<ILogger<GetContentBundlesByAgeGroupQuery>> _logger;

    public GetContentBundlesByAgeGroupQueryHandlerTests()
    {
        _repository = new Mock<IContentBundleRepository>();
        _logger = new Mock<ILogger<GetContentBundlesByAgeGroupQuery>>();
    }

    [Fact]
    public async Task Handle_WithValidAgeGroup_ReturnsBundles()
    {
        var bundles = new List<ContentBundle>
        {
            new() { Id = "bundle-1", Title = "Adventure Pack" },
            new() { Id = "bundle-2", Title = "Mystery Pack" }
        };
        _repository.Setup(r => r.GetByAgeGroupAsync("6-9", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundles);

        var result = await GetContentBundlesByAgeGroupQueryHandler.Handle(
            new GetContentBundlesByAgeGroupQuery("6-9"), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithEmptyAgeGroup_ThrowsValidationException()
    {
        var act = () => GetContentBundlesByAgeGroupQueryHandler.Handle(
            new GetContentBundlesByAgeGroupQuery(""), _repository.Object, _logger.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithNoResults_ReturnsEmptyCollection()
    {
        _repository.Setup(r => r.GetByAgeGroupAsync("99-100", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentBundle>());

        var result = await GetContentBundlesByAgeGroupQueryHandler.Handle(
            new GetContentBundlesByAgeGroupQuery("99-100"), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
