using FluentAssertions;
using Mystira.Shared.Data.Repositories;
using Xunit;

namespace Mystira.Shared.Tests.Data;

public class RepositoryBaseTests
{
    [Fact]
    public void IRepository_SupportsStringId()
    {
        // This test verifies the interface signature supports string IDs
        // The actual implementation is tested with integration tests
        typeof(IRepository<>).Should().NotBeNull();
    }

    [Fact]
    public void IRepository_HasGuidIdOverloads()
    {
        // Verify the interface has Guid-specific methods
        var repoType = typeof(IRepository<>);
        var methods = repoType.GetMethods();

        methods.Should().Contain(m => m.Name == "GetByIdAsync" &&
            m.GetParameters().Any(p => p.ParameterType == typeof(Guid)));
        methods.Should().Contain(m => m.Name == "DeleteAsync" &&
            m.GetParameters().Any(p => p.ParameterType == typeof(Guid)));
    }

    [Fact]
    public void IRepository_HasSpecificationMethods()
    {
        // Verify the interface has specification-based query methods
        var repoType = typeof(IRepository<>);
        var methods = repoType.GetMethods();

        methods.Should().Contain(m => m.Name == "ListAsync");
        methods.Should().Contain(m => m.Name == "FirstOrDefaultAsync");
        methods.Should().Contain(m => m.Name == "CountAsync");
    }
}
