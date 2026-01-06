using Mystira.Contracts.App.Requests.Scenarios;
using Mystira.Domain.Models;

namespace Mystira.Admin.Api.Tests.Infrastructure;

/// <summary>
/// Builder class for creating test data with sensible defaults.
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// Creates a ScenarioQueryRequest builder.
    /// </summary>
    public static ScenarioQueryRequestBuilder ScenarioQuery() => new();

    /// <summary>
    /// Creates a CreateScenarioRequest builder.
    /// </summary>
    public static CreateScenarioRequestBuilder CreateScenario() => new();

    /// <summary>
    /// Creates a Scenario builder.
    /// </summary>
    public static ScenarioBuilder Scenario() => new();
}

/// <summary>
/// Builder for ScenarioQueryRequest.
/// </summary>
public class ScenarioQueryRequestBuilder
{
    private int _page = 1;
    private int _pageSize = 10;
    private string? _searchTerm;
    private string? _ageGroup;
    private bool? _isFeatured;

    public ScenarioQueryRequestBuilder WithPage(int page)
    {
        _page = page;
        return this;
    }

    public ScenarioQueryRequestBuilder WithPageSize(int pageSize)
    {
        _pageSize = pageSize;
        return this;
    }

    public ScenarioQueryRequestBuilder WithSearchTerm(string searchTerm)
    {
        _searchTerm = searchTerm;
        return this;
    }

    public ScenarioQueryRequestBuilder WithAgeGroup(string ageGroup)
    {
        _ageGroup = ageGroup;
        return this;
    }

    public ScenarioQueryRequestBuilder OnlyFeatured()
    {
        _isFeatured = true;
        return this;
    }

    public ScenarioQueryRequest Build()
    {
        return new ScenarioQueryRequest
        {
            Page = _page,
            PageSize = _pageSize,
            SearchTerm = _searchTerm,
            AgeGroup = _ageGroup,
            IsFeatured = _isFeatured
        };
    }
}

/// <summary>
/// Builder for CreateScenarioRequest.
/// </summary>
public class CreateScenarioRequestBuilder
{
    private string _title = "Test Scenario";
    private string _description = "A test scenario for unit testing";
    private string _ageGroup = "all-ages";
    private bool _isFeatured;
    private List<string> _tags = new() { "test", "integration" };
    private string? _thumbnailUrl;

    public CreateScenarioRequestBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public CreateScenarioRequestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public CreateScenarioRequestBuilder WithAgeGroup(string ageGroup)
    {
        _ageGroup = ageGroup;
        return this;
    }

    public CreateScenarioRequestBuilder AsFeatured()
    {
        _isFeatured = true;
        return this;
    }

    public CreateScenarioRequestBuilder WithTags(params string[] tags)
    {
        _tags = tags.ToList();
        return this;
    }

    public CreateScenarioRequestBuilder WithThumbnail(string thumbnailUrl)
    {
        _thumbnailUrl = thumbnailUrl;
        return this;
    }

    public CreateScenarioRequest Build()
    {
        return new CreateScenarioRequest
        {
            Title = _title,
            Description = _description,
            AgeGroup = _ageGroup,
            IsFeatured = _isFeatured,
            Tags = _tags,
            ThumbnailUrl = _thumbnailUrl
        };
    }
}

/// <summary>
/// Builder for Scenario domain model.
/// </summary>
public class ScenarioBuilder
{
    private string _id = Guid.NewGuid().ToString();
    private string _title = "Test Scenario";
    private string _description = "A test scenario";
    private string _ageGroup = "all-ages";
    private bool _isFeatured;
    private List<string> _tags = new() { "test" };
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;

    public ScenarioBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public ScenarioBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public ScenarioBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ScenarioBuilder WithAgeGroup(string ageGroup)
    {
        _ageGroup = ageGroup;
        return this;
    }

    public ScenarioBuilder AsFeatured()
    {
        _isFeatured = true;
        return this;
    }

    public ScenarioBuilder WithTags(params string[] tags)
    {
        _tags = tags.ToList();
        return this;
    }

    public ScenarioBuilder CreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public Scenario Build()
    {
        return new Scenario
        {
            Id = _id,
            Title = _title,
            Description = _description,
            AgeGroup = _ageGroup,
            IsFeatured = _isFeatured,
            Tags = _tags,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt
        };
    }
}
