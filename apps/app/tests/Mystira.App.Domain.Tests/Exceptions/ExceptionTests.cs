using Mystira.Shared.Exceptions;

namespace Mystira.App.Domain.Tests.Exceptions;

public class NotFoundExceptionTests
{
    [Fact]
    public void Constructor_WithResourceTypeAndId_SetsPropertiesCorrectly()
    {
        // Arrange
        const string resourceType = "Scenario";
        const string resourceId = "123";

        // Act
        var exception = new NotFoundException(resourceType, resourceId);

        // Assert
        exception.ResourceType.Should().Be(resourceType);
        exception.ResourceId.Should().Be(resourceId);
        exception.ErrorCode.Should().Be("RESOURCE_NOT_FOUND");
        exception.Message.Should().Contain(resourceType);
        exception.Message.Should().Contain(resourceId);
        exception.Details.Should().ContainKey("resourceType");
        exception.Details.Should().ContainKey("resourceId");
    }

    [Fact]
    public void Constructor_WithCustomMessage_UsesCustomMessage()
    {
        // Arrange
        const string resourceType = "Account";
        const string resourceId = "456";
        const string customMessage = "The account has been deactivated.";

        // Act
        var exception = new NotFoundException(resourceType, resourceId, customMessage);

        // Assert
        exception.Message.Should().Be(customMessage);
        exception.ResourceType.Should().Be(resourceType);
        exception.ResourceId.Should().Be(resourceId);
    }
}

public class BusinessRuleExceptionTests
{
    [Fact]
    public void Constructor_WithRuleNameAndMessage_SetsPropertiesCorrectly()
    {
        // Arrange
        const string ruleName = "MaxProfilesPerAccount";
        const string message = "Cannot create more than 5 profiles per account.";

        // Act
        var exception = new BusinessRuleException(ruleName, message);

        // Assert
        exception.RuleName.Should().Be(ruleName);
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be("BUSINESS_RULE_VIOLATION");
        exception.Details.Should().ContainKey("ruleName");
        exception.Details!["ruleName"].Should().Be(ruleName);
    }

    [Fact]
    public void Constructor_WithContext_IncludesContextInDetails()
    {
        // Arrange
        const string ruleName = "AgeRestriction";
        const string message = "Content is not appropriate for this age group.";
        var context = new Dictionary<string, object>
        {
            ["contentAgeGroup"] = "13-18",
            ["viewerAgeGroup"] = "6-9"
        };

        // Act
        var exception = new BusinessRuleException(ruleName, message, context);

        // Assert
        exception.Details.Should().ContainKey("ruleName");
        exception.Details.Should().ContainKey("contentAgeGroup");
        exception.Details.Should().ContainKey("viewerAgeGroup");
        exception.Details!["contentAgeGroup"].Should().Be("13-18");
    }
}

public class ValidationExceptionTests
{
    [Fact]
    public void Constructor_WithMessageOnly_SetsEmptyErrors()
    {
        // Arrange
        const string message = "Validation failed.";

        // Act
        var exception = new ValidationException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be("VALIDATION_FAILED");
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithFieldAndMessage_CreatesSingleError()
    {
        // Arrange
        const string field = "Email";
        const string message = "Email format is invalid.";

        // Act
        var exception = new ValidationException(field, message);

        // Assert
        exception.Errors.Should().HaveCount(1);
        exception.Errors[0].Field.Should().Be(field);
        exception.Errors[0].Message.Should().Be(message);
        exception.Details.Should().ContainKey("field");
    }

    [Fact]
    public void Constructor_WithMultipleErrors_StoresAllErrors()
    {
        // Arrange
        var errors = new[]
        {
            new ValidationError("Name", "Name is required."),
            new ValidationError("Email", "Email format is invalid."),
            new ValidationError("Age", "Age must be positive.")
        };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.Errors.Should().HaveCount(3);
        exception.Message.Should().Contain("validation errors");
        exception.Details.Should().ContainKey("errors");
    }
}

public class ConflictExceptionTests
{
    [Fact]
    public void Constructor_WithMessageOnly_SetsUnknownResourceType()
    {
        // Arrange
        const string message = "A conflict occurred.";

        // Act
        var exception = new ConflictException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be("RESOURCE_CONFLICT");
        exception.ResourceType.Should().Be("Unknown");
        exception.ConflictingField.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithResourceType_SetsResourceType()
    {
        // Arrange
        const string resourceType = "Account";
        const string message = "An account with this email already exists.";

        // Act
        var exception = new ConflictException(resourceType, message);

        // Assert
        exception.ResourceType.Should().Be(resourceType);
        exception.Details.Should().ContainKey("resourceType");
    }

    [Fact]
    public void Constructor_WithConflictingField_SetsAllProperties()
    {
        // Arrange
        const string resourceType = "UserProfile";
        const string conflictingField = "Email";
        const string message = "A profile with this email already exists.";

        // Act
        var exception = new ConflictException(resourceType, conflictingField, message);

        // Assert
        exception.ResourceType.Should().Be(resourceType);
        exception.ConflictingField.Should().Be(conflictingField);
        exception.Details.Should().ContainKey("conflictingField");
    }
}

public class ForbiddenExceptionTests
{
    [Fact]
    public void Constructor_WithMessageOnly_SetsBasicProperties()
    {
        // Arrange
        const string message = "Access denied.";

        // Act
        var exception = new ForbiddenException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be("ACCESS_FORBIDDEN");
        exception.Resource.Should().BeNull();
        exception.RequiredPermission.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithResource_SetsResource()
    {
        // Arrange
        const string resource = "AdminDashboard";
        const string message = "You do not have access to the admin dashboard.";

        // Act
        var exception = new ForbiddenException(resource, message);

        // Assert
        exception.Resource.Should().Be(resource);
        exception.Details.Should().ContainKey("resource");
    }

    [Fact]
    public void Constructor_WithRequiredPermission_SetsAllProperties()
    {
        // Arrange
        const string resource = "ScenarioEditor";
        const string requiredPermission = "scenarios:write";
        const string message = "You need write permission to edit scenarios.";

        // Act
        var exception = new ForbiddenException(resource, requiredPermission, message);

        // Assert
        exception.Resource.Should().Be(resource);
        exception.RequiredPermission.Should().Be(requiredPermission);
        exception.Details.Should().ContainKey("requiredPermission");
    }
}

public class ValidationErrorTests
{
    [Fact]
    public void Record_SupportsEquality()
    {
        // Arrange
        var error1 = new ValidationError("Email", "Invalid format");
        var error2 = new ValidationError("Email", "Invalid format");
        var error3 = new ValidationError("Name", "Required");

        // Assert
        error1.Should().Be(error2);
        error1.Should().NotBe(error3);
    }

    [Fact]
    public void Record_SupportsDeconstruction()
    {
        // Arrange
        var error = new ValidationError("Password", "Too short");

        // Act
        var (field, message) = error;

        // Assert
        field.Should().Be("Password");
        message.Should().Be("Too short");
    }
}
