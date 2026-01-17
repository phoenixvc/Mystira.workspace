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
        // Note: For open generics, we check the interface hierarchy directly
        var interfaces = typeof(IRepository<>).GetInterfaces();
        interfaces.Should().Contain(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepositoryBase<>));
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
        // Verify IRepositoryBase is in the inheritance chain
        // Note: GetMethods() on an interface only returns directly declared methods,
        // not inherited ones. We verify inheritance by checking the interface hierarchy.
        var interfaces = typeof(IRepository<>).GetInterfaces();
        var repositoryBaseInterface = interfaces.FirstOrDefault(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepositoryBase<>));

        repositoryBaseInterface.Should().NotBeNull("IRepository should extend IRepositoryBase");

        // Verify IRepositoryBase has the expected methods
        var baseMethods = typeof(IRepositoryBase<>).GetMethods();
        baseMethods.Should().Contain(m => m.Name == "AddAsync");
        baseMethods.Should().Contain(m => m.Name == "AddRangeAsync");
        baseMethods.Should().Contain(m => m.Name == "UpdateAsync");
        baseMethods.Should().Contain(m => m.Name == "DeleteAsync");
        baseMethods.Should().Contain(m => m.Name == "SaveChangesAsync");
        baseMethods.Should().Contain(m => m.Name == "ListAsync");
        baseMethods.Should().Contain(m => m.Name == "CountAsync");
        baseMethods.Should().Contain(m => m.Name == "FirstOrDefaultAsync");
    }
}
