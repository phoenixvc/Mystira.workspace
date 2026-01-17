using Ardalis.Specification;
using FluentAssertions;
using Mystira.Application.Ports.Data;
using Xunit;

namespace Mystira.Shared.Tests.Data;

public class RepositoryBaseTests
{
    [Fact]
    public void IRepository_ExtendsIRepositoryBase()
    {
        // Verify that IRepository extends IRepositoryBase from Ardalis.Specification
        typeof(IRepository<>).Should().Implement(typeof(IRepositoryBase<>));
    }

    [Fact]
    public void IRepository_HasStringIdMethods()
    {
        // Verify the interface has string ID-specific methods
        var repoType = typeof(IRepository<>);
        var methods = repoType.GetMethods();

        methods.Should().Contain(m => m.Name == "GetByIdAsync" &&
            m.GetParameters().Any(p => p.ParameterType == typeof(string)));
        methods.Should().Contain(m => m.Name == "DeleteAsync" &&
            m.GetParameters().Any(p => p.ParameterType == typeof(string)));
        methods.Should().Contain(m => m.Name == "ExistsAsync" &&
            m.GetParameters().Any(p => p.ParameterType == typeof(string)));
    }

    [Fact]
    public void IRepository_HasGuidIdMethods()
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

        methods.Should().Contain(m => m.Name == "GetBySpecAsync");
        methods.Should().Contain(m => m.Name == "StreamAsync");
    }

    [Fact]
    public void IRepository_HasStreamingMethods()
    {
        // Verify the interface has streaming methods
        var repoType = typeof(IRepository<>);
        var methods = repoType.GetMethods();

        methods.Should().Contain(m => m.Name == "StreamAllAsync");
        methods.Should().Contain(m => m.Name == "StreamAsync");
    }

    [Fact]
    public void IRepository_InheritsIRepositoryBaseMethods()
    {
        // Verify inherited IRepositoryBase methods are available
        var repoType = typeof(IRepository<>);
        var allMethods = repoType.GetMethods();

        // Methods from IRepositoryBase
        allMethods.Should().Contain(m => m.Name == "AddAsync");
        allMethods.Should().Contain(m => m.Name == "AddRangeAsync");
        allMethods.Should().Contain(m => m.Name == "UpdateAsync");
        allMethods.Should().Contain(m => m.Name == "DeleteAsync");
        allMethods.Should().Contain(m => m.Name == "SaveChangesAsync");
        allMethods.Should().Contain(m => m.Name == "ListAsync");
        allMethods.Should().Contain(m => m.Name == "CountAsync");
        allMethods.Should().Contain(m => m.Name == "FirstOrDefaultAsync");
    }
}
