using LifeOS.Core.IntegrationConnectors.GoogleWorkspace;
using LifeOS.Core.IntegrationControlCentre;
using LifeOS.Core.IntegrationInbox;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class GoogleWorkspaceGroup51Tests
{
    [Fact]
    public void CompleteCatalogue_ContainsFiveReadOnlyApis()
    {
        GoogleWorkspaceProviderConfiguration configuration = new();

        configuration.Validate();

        Assert.Equal(5, configuration.Catalogue.Count);
        Assert.Equal(5, configuration.RequestedReadOnlyScopes.Count);
        Assert.All(configuration.RequestedReadOnlyScopes, scope =>
            Assert.EndsWith(".readonly", scope, StringComparison.Ordinal));
    }

    [Fact]
    public void CompleteCatalogue_UsesOneProjectBoundary()
    {
        Assert.Equal(
            "One complete LifeOS Google Cloud project",
            GoogleWorkspaceProviderConfiguration.ProjectBoundary);
        Assert.Contains(
            "reuse the same project",
            GoogleWorkspaceProviderConfiguration.FutureClientBoundary,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ControlCentreMigration_ConnectsFiveCapabilities()
    {
        DateTimeOffset now =
            DateTimeOffset.Parse("2026-07-17T09:30:00Z");
        IntegrationControlCentreState state =
            IntegrationControlCentreSeed.CreateFictional(now);

        Group51GoogleWorkspaceMigration.Apply(state, now);

        IntegrationProviderProfile provider =
            Assert.Single(state.Providers, item => item.Id == "google");
        ConnectedIntegrationAccount account =
            Assert.Single(state.Accounts, item => item.Id == "google-personal");

        Assert.False(provider.IsFictional);
        Assert.Equal(5, provider.Capabilities.Count);
        Assert.Equal(5, account.Permissions.Count);
        Assert.All(account.Permissions, permission =>
            Assert.Equal(
                IntegrationPermissionState.Granted,
                permission.State));
    }

    [Fact]
    public void InboxMigration_AddsAllFiveCandidateTypesIdempotently()
    {
        DateTimeOffset now =
            DateTimeOffset.Parse("2026-07-17T09:30:00Z");
        IntegrationInboxV9State state =
            IntegrationInboxV9Seed.CreateFictional(now);

        Group51IntegrationInboxMigration.Apply(state, now);
        int firstCount = state.Candidates.Count;
        Group51IntegrationInboxMigration.Apply(state, now);

        Assert.Equal(firstCount, state.Candidates.Count);
        Assert.Contains(state.Candidates, item => item.Id == "group51-gmail");
        Assert.Contains(state.Candidates, item => item.Id == "group51-google-calendar");
        Assert.Contains(state.Candidates, item => item.Id == "group51-google-drive");
        Assert.Contains(state.Candidates, item => item.Id == "group51-google-contact");
        Assert.Contains(state.Candidates, item => item.Id == "group51-google-task");
        Assert.Contains(state.Candidates, item =>
            item.Id == "group51-google-revoked" &&
            item.Status == IntegrationCandidateStatus.SourceRemoved);
    }

    [Fact]
    public void DriveCandidate_DoesNotDownloadBody()
    {
        GoogleWorkspaceIdentity identity =
            new("google-personal", "Codie Shannon", "redacted", "Personal");
        GoogleDriveRecord record = new(
            "drive-1",
            "Evidence.docx",
            ".docx",
            100,
            DateTimeOffset.Parse("2026-07-17T09:00:00Z"),
            "Selected folder",
            "Codie Shannon",
            "redacted-change");

        FileDocumentCandidateDraft candidate =
            GoogleWorkspaceCandidateFactory.Drive(identity, record);

        Assert.Equal("google-drive", candidate.CapabilityId);
        Assert.Equal("Not downloaded", candidate.AdditionalFields["file-body"]);
    }
}
