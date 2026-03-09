using Mystira.App.Domain.Models;

namespace Mystira.App.Domain.Tests.Models;

public class ParentalConsentTests
{
    #region Default State Tests

    [Fact]
    public void NewConsent_HasPendingStatus()
    {
        // Act
        var consent = new ParentalConsent();

        // Assert
        consent.Status.Should().Be(ConsentStatus.Pending);
    }

    [Fact]
    public void NewConsent_HasNonEmptyId()
    {
        // Act
        var consent = new ParentalConsent();

        // Assert
        consent.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void NewConsent_HasNoVerificationMethod()
    {
        // Act
        var consent = new ParentalConsent();

        // Assert
        consent.VerificationMethod.Should().Be(ConsentVerificationMethod.None);
    }

    [Fact]
    public void NewConsent_HasNullTimestamps()
    {
        // Act
        var consent = new ParentalConsent();

        // Assert
        consent.ConsentedAt.Should().BeNull();
        consent.VerifiedAt.Should().BeNull();
        consent.RevokedAt.Should().BeNull();
    }

    [Fact]
    public void NewConsent_HasDefaultPrivacyPolicyVersion()
    {
        // Act
        var consent = new ParentalConsent();

        // Assert
        consent.PrivacyPolicyVersion.Should().Be("1.0");
    }

    [Fact]
    public void NewConsent_SetsCreatedAtToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var consent = new ParentalConsent();

        // Assert
        var after = DateTime.UtcNow;
        consent.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    #endregion

    #region Approve Tests

    [Fact]
    public void Approve_SetsStatusToVerified()
    {
        // Arrange
        var consent = new ParentalConsent();

        // Act
        consent.Approve(ConsentVerificationMethod.Email);

        // Assert
        consent.Status.Should().Be(ConsentStatus.Verified);
    }

    [Fact]
    public void Approve_SetsVerificationMethod()
    {
        // Arrange
        var consent = new ParentalConsent();

        // Act
        consent.Approve(ConsentVerificationMethod.CreditCard);

        // Assert
        consent.VerificationMethod.Should().Be(ConsentVerificationMethod.CreditCard);
    }

    [Fact]
    public void Approve_SetsConsentedAt()
    {
        // Arrange
        var consent = new ParentalConsent();
        var before = DateTime.UtcNow;

        // Act
        consent.Approve(ConsentVerificationMethod.Email);

        // Assert
        var after = DateTime.UtcNow;
        consent.ConsentedAt.Should().NotBeNull();
        consent.ConsentedAt!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Approve_SetsVerifiedAt()
    {
        // Arrange
        var consent = new ParentalConsent();
        var before = DateTime.UtcNow;

        // Act
        consent.Approve(ConsentVerificationMethod.GovernmentId);

        // Assert
        var after = DateTime.UtcNow;
        consent.VerifiedAt.Should().NotBeNull();
        consent.VerifiedAt!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Approve_SetsUpdatedAt()
    {
        // Arrange
        var consent = new ParentalConsent();
        var originalUpdatedAt = consent.UpdatedAt;

        // Act
        consent.Approve(ConsentVerificationMethod.Email);

        // Assert
        consent.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
    }

    [Theory]
    [InlineData(ConsentVerificationMethod.Email)]
    [InlineData(ConsentVerificationMethod.CreditCard)]
    [InlineData(ConsentVerificationMethod.GovernmentId)]
    [InlineData(ConsentVerificationMethod.VideoCall)]
    [InlineData(ConsentVerificationMethod.SignedForm)]
    public void Approve_WithEachMethod_SetsCorrectMethod(ConsentVerificationMethod method)
    {
        // Arrange
        var consent = new ParentalConsent();

        // Act
        consent.Approve(method);

        // Assert
        consent.VerificationMethod.Should().Be(method);
        consent.Status.Should().Be(ConsentStatus.Verified);
    }

    #endregion

    #region Revoke Tests

    [Fact]
    public void Revoke_SetsStatusToRevoked()
    {
        // Arrange
        var consent = new ParentalConsent();
        consent.Approve(ConsentVerificationMethod.Email);

        // Act
        consent.Revoke();

        // Assert
        consent.Status.Should().Be(ConsentStatus.Revoked);
    }

    [Fact]
    public void Revoke_SetsRevokedAt()
    {
        // Arrange
        var consent = new ParentalConsent();
        consent.Approve(ConsentVerificationMethod.Email);
        var before = DateTime.UtcNow;

        // Act
        consent.Revoke();

        // Assert
        var after = DateTime.UtcNow;
        consent.RevokedAt.Should().NotBeNull();
        consent.RevokedAt!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Revoke_SetsUpdatedAt()
    {
        // Arrange
        var consent = new ParentalConsent();
        consent.Approve(ConsentVerificationMethod.Email);
        var updatedAtAfterApproval = consent.UpdatedAt;

        // Act
        consent.Revoke();

        // Assert
        consent.UpdatedAt.Should().BeOnOrAfter(updatedAtAfterApproval);
    }

    [Fact]
    public void Revoke_FromPendingStatus_SetsStatusToRevoked()
    {
        // Arrange
        var consent = new ParentalConsent();

        // Act
        consent.Revoke();

        // Assert
        consent.Status.Should().Be(ConsentStatus.Revoked);
        consent.RevokedAt.Should().NotBeNull();
    }

    #endregion

    #region Deny Tests

    [Fact]
    public void Deny_SetsStatusToDenied()
    {
        // Arrange
        var consent = new ParentalConsent();

        // Act
        consent.Deny();

        // Assert
        consent.Status.Should().Be(ConsentStatus.Denied);
    }

    [Fact]
    public void Deny_SetsUpdatedAt()
    {
        // Arrange
        var consent = new ParentalConsent();
        var originalUpdatedAt = consent.UpdatedAt;

        // Act
        consent.Deny();

        // Assert
        consent.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
    }

    [Fact]
    public void Deny_DoesNotSetRevokedAt()
    {
        // Arrange
        var consent = new ParentalConsent();

        // Act
        consent.Deny();

        // Assert
        consent.RevokedAt.Should().BeNull();
    }

    [Fact]
    public void Deny_DoesNotSetConsentedAt()
    {
        // Arrange
        var consent = new ParentalConsent();

        // Act
        consent.Deny();

        // Assert
        consent.ConsentedAt.Should().BeNull();
    }

    #endregion

    #region IsActive Tests

    [Fact]
    public void IsActive_WhenVerifiedAndNotRevoked_ReturnsTrue()
    {
        // Arrange
        var consent = new ParentalConsent();
        consent.Approve(ConsentVerificationMethod.Email);

        // Act & Assert
        consent.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenPending_ReturnsFalse()
    {
        // Arrange
        var consent = new ParentalConsent();

        // Act & Assert
        consent.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenDenied_ReturnsFalse()
    {
        // Arrange
        var consent = new ParentalConsent();
        consent.Deny();

        // Act & Assert
        consent.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenRevoked_ReturnsFalse()
    {
        // Arrange
        var consent = new ParentalConsent();
        consent.Approve(ConsentVerificationMethod.Email);
        consent.Revoke();

        // Act & Assert
        consent.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenVerifiedButRevokedAtIsSet_ReturnsFalse()
    {
        // Arrange - simulate edge case where status is Verified but RevokedAt was set
        var consent = new ParentalConsent
        {
            Status = ConsentStatus.Verified,
            RevokedAt = DateTime.UtcNow
        };

        // Act & Assert
        consent.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenStatusIsNotVerifiedAndRevokedAtIsNull_ReturnsFalse()
    {
        // Arrange
        var consent = new ParentalConsent
        {
            Status = ConsentStatus.Expired,
            RevokedAt = null
        };

        // Act & Assert
        consent.IsActive.Should().BeFalse();
    }

    #endregion

    #region IsTokenExpired Tests

    [Fact]
    public void IsTokenExpired_WhenExpiresAtIsInThePast_ReturnsTrue()
    {
        // Arrange
        var consent = new ParentalConsent
        {
            VerificationTokenExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };

        // Act & Assert
        consent.IsTokenExpired.Should().BeTrue();
    }

    [Fact]
    public void IsTokenExpired_WhenExpiresAtIsInTheFuture_ReturnsFalse()
    {
        // Arrange
        var consent = new ParentalConsent
        {
            VerificationTokenExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        // Act & Assert
        consent.IsTokenExpired.Should().BeFalse();
    }

    [Fact]
    public void IsTokenExpired_WhenExpiresAtIsNull_ReturnsFalse()
    {
        // Arrange
        var consent = new ParentalConsent
        {
            VerificationTokenExpiresAt = null
        };

        // Act & Assert
        consent.IsTokenExpired.Should().BeFalse();
    }

    [Fact]
    public void IsTokenExpired_WhenExpiresAtIsFarInThePast_ReturnsTrue()
    {
        // Arrange
        var consent = new ParentalConsent
        {
            VerificationTokenExpiresAt = DateTime.UtcNow.AddDays(-30)
        };

        // Act & Assert
        consent.IsTokenExpired.Should().BeTrue();
    }

    #endregion

    #region Workflow Integration Tests

    [Fact]
    public void FullWorkflow_ApproveAndRevoke_TransitionsCorrectly()
    {
        // Arrange
        var consent = new ParentalConsent
        {
            ParentEmailHash = "abc123hash",
            ChildProfileId = "child-1",
            ChildDisplayName = "StarExplorer"
        };

        // Assert initial state
        consent.Status.Should().Be(ConsentStatus.Pending);
        consent.IsActive.Should().BeFalse();

        // Act - Approve
        consent.Approve(ConsentVerificationMethod.Email);

        // Assert approved state
        consent.Status.Should().Be(ConsentStatus.Verified);
        consent.IsActive.Should().BeTrue();
        consent.ConsentedAt.Should().NotBeNull();
        consent.VerifiedAt.Should().NotBeNull();

        // Act - Revoke
        consent.Revoke();

        // Assert revoked state
        consent.Status.Should().Be(ConsentStatus.Revoked);
        consent.IsActive.Should().BeFalse();
        consent.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public void TwoConsents_HaveDifferentIds()
    {
        // Act
        var consent1 = new ParentalConsent();
        var consent2 = new ParentalConsent();

        // Assert
        consent1.Id.Should().NotBe(consent2.Id);
    }

    #endregion
}

public class DataDeletionRequestTests
{
    #region Default State Tests

    [Fact]
    public void NewRequest_HasPendingStatus()
    {
        // Act
        var request = new DataDeletionRequest();

        // Assert
        request.Status.Should().Be(DeletionStatus.Pending);
    }

    [Fact]
    public void NewRequest_HasNonEmptyId()
    {
        // Act
        var request = new DataDeletionRequest();

        // Assert
        request.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void NewRequest_HasDefaultDeletionScope()
    {
        // Act
        var request = new DataDeletionRequest();

        // Assert
        request.DeletionScope.Should().Contain("CosmosDB");
        request.DeletionScope.Should().Contain("BlobStorage");
        request.DeletionScope.Should().Contain("Logs");
        request.DeletionScope.Should().HaveCount(3);
    }

    [Fact]
    public void NewRequest_HasEmptyAuditTrail()
    {
        // Act
        var request = new DataDeletionRequest();

        // Assert
        request.AuditTrail.Should().BeEmpty();
    }

    [Fact]
    public void NewRequest_HasNullCompletedAt()
    {
        // Act
        var request = new DataDeletionRequest();

        // Assert
        request.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void NewRequest_HasConfirmationEmailSentFalse()
    {
        // Act
        var request = new DataDeletionRequest();

        // Assert
        request.ConfirmationEmailSent.Should().BeFalse();
    }

    [Fact]
    public void NewRequest_SetsCreatedAtToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var request = new DataDeletionRequest();

        // Assert
        var after = DateTime.UtcNow;
        request.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    #endregion

    #region AddAuditEntry Tests

    [Fact]
    public void AddAuditEntry_AddsEntryToAuditTrail()
    {
        // Arrange
        var request = new DataDeletionRequest();

        // Act
        request.AddAuditEntry("DataDeleted", "parentHash123");

        // Assert
        request.AuditTrail.Should().HaveCount(1);
        request.AuditTrail[0].Action.Should().Be("DataDeleted");
        request.AuditTrail[0].PerformedByHash.Should().Be("parentHash123");
    }

    [Fact]
    public void AddAuditEntry_SetsTimestampOnEntry()
    {
        // Arrange
        var request = new DataDeletionRequest();
        var before = DateTime.UtcNow;

        // Act
        request.AddAuditEntry("BlobsDeleted", "systemHash");

        // Assert
        var after = DateTime.UtcNow;
        request.AuditTrail[0].Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void AddAuditEntry_UpdatesUpdatedAt()
    {
        // Arrange
        var request = new DataDeletionRequest();
        var originalUpdatedAt = request.UpdatedAt;

        // Act
        request.AddAuditEntry("SoftDeleteStarted", "systemHash");

        // Assert
        request.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
    }

    [Fact]
    public void AddAuditEntry_MultipleTimes_AccumulatesEntries()
    {
        // Arrange
        var request = new DataDeletionRequest();

        // Act
        request.AddAuditEntry("CosmosDBDeleted", "systemHash");
        request.AddAuditEntry("BlobStorageDeleted", "systemHash");
        request.AddAuditEntry("LogsPurged", "systemHash");

        // Assert
        request.AuditTrail.Should().HaveCount(3);
        request.AuditTrail[0].Action.Should().Be("CosmosDBDeleted");
        request.AuditTrail[1].Action.Should().Be("BlobStorageDeleted");
        request.AuditTrail[2].Action.Should().Be("LogsPurged");
    }

    [Fact]
    public void AddAuditEntry_PreservesExistingEntries()
    {
        // Arrange
        var request = new DataDeletionRequest();
        request.AddAuditEntry("First", "hash1");

        // Act
        request.AddAuditEntry("Second", "hash2");

        // Assert
        request.AuditTrail[0].Action.Should().Be("First");
        request.AuditTrail[0].PerformedByHash.Should().Be("hash1");
        request.AuditTrail[1].Action.Should().Be("Second");
        request.AuditTrail[1].PerformedByHash.Should().Be("hash2");
    }

    #endregion

    #region Complete Tests

    [Fact]
    public void Complete_SetsStatusToCompleted()
    {
        // Arrange
        var request = new DataDeletionRequest();

        // Act
        request.Complete();

        // Assert
        request.Status.Should().Be(DeletionStatus.Completed);
    }

    [Fact]
    public void Complete_SetsCompletedAt()
    {
        // Arrange
        var request = new DataDeletionRequest();
        var before = DateTime.UtcNow;

        // Act
        request.Complete();

        // Assert
        var after = DateTime.UtcNow;
        request.CompletedAt.Should().NotBeNull();
        request.CompletedAt!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Complete_SetsUpdatedAt()
    {
        // Arrange
        var request = new DataDeletionRequest();
        var originalUpdatedAt = request.UpdatedAt;

        // Act
        request.Complete();

        // Assert
        request.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
    }

    #endregion

    #region Workflow Integration Tests

    [Fact]
    public void FullWorkflow_AddAuditEntriesAndComplete_TracksCorrectly()
    {
        // Arrange
        var request = new DataDeletionRequest
        {
            ChildProfileId = "child-1",
            RequestedBy = DeletionRequestSource.Parent,
            ScheduledDeletionAt = DateTime.UtcNow.AddDays(7)
        };

        // Assert initial state
        request.Status.Should().Be(DeletionStatus.Pending);
        request.AuditTrail.Should().BeEmpty();
        request.CompletedAt.Should().BeNull();

        // Act - add audit entries during deletion process
        request.AddAuditEntry("SoftDeleteInitiated", "systemHash");
        request.AddAuditEntry("CosmosDBRecordsDeleted", "systemHash");
        request.AddAuditEntry("BlobStorageCleaned", "systemHash");
        request.AddAuditEntry("LogsRedacted", "systemHash");

        // Assert intermediate state
        request.AuditTrail.Should().HaveCount(4);

        // Act - complete the deletion
        request.Complete();

        // Assert final state
        request.Status.Should().Be(DeletionStatus.Completed);
        request.CompletedAt.Should().NotBeNull();
        request.AuditTrail.Should().HaveCount(4);
    }

    [Fact]
    public void TwoRequests_HaveDifferentIds()
    {
        // Act
        var request1 = new DataDeletionRequest();
        var request2 = new DataDeletionRequest();

        // Assert
        request1.Id.Should().NotBe(request2.Id);
    }

    [Theory]
    [InlineData(DeletionRequestSource.Parent)]
    [InlineData(DeletionRequestSource.System)]
    [InlineData(DeletionRequestSource.Support)]
    public void Request_CanBeCreatedWithAnySource(DeletionRequestSource source)
    {
        // Act
        var request = new DataDeletionRequest
        {
            RequestedBy = source
        };

        // Assert
        request.RequestedBy.Should().Be(source);
    }

    #endregion
}
