using LifeOS.Core.IntegrationControlCentre;
using LifeOS.Core.IntegrationInbox;
using LifeOS.Core.MicrosoftProvider;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class MicrosoftFilesGroup49Tests
{
    [Fact]
    public void Group49ScopesReuseMicrosoftIdentityAndRemainReadOnly()
    {
        IReadOnlyList<string> scopes = MicrosoftProviderConfiguration.Group49RequestedScopes;
        Assert.Contains("Files.Read", scopes);
        Assert.Contains("Sites.Read.All", scopes);
        Assert.Contains("Mail.Read", scopes);
        Assert.DoesNotContain("Files.ReadWrite", scopes);
        Assert.DoesNotContain("Files.ReadWrite.All", scopes);
        Assert.DoesNotContain("Sites.ReadWrite.All", scopes);
    }

    [Fact]
    public void FileCandidateCarriesMetadataProvenanceAndNoBodyDownload()
    {
        MicrosoftProviderAccount account = Account();
        MicrosoftFileDescriptor item = Item("file-1", "Proposal.pdf", MicrosoftFileSourceState.Current);
        IntegrationCandidate candidate = IntegrationCandidateNormalizer.Normalize(
            MicrosoftFilesCandidateFactory.CreateDraft(account, item),
            DateTimeOffset.Parse("2026-07-17T02:00:00Z"));

        Assert.Equal(IntegrationCandidateType.FileDocument, candidate.Type);
        Assert.Equal("onedrive-files", candidate.Provenance.CapabilityId);
        Assert.Contains(candidate.Fields, f => f.Key == "etag" && f.Value == "etag-1");
        Assert.Contains(candidate.Fields, f => f.Key == "owner" && f.Value == "Alex Morgan");
        Assert.Contains(candidate.Fields, f => f.Key == "body-downloaded" && f.Value == "false");
    }

    [Fact]
    public async Task SelectedDriveBoundaryIsEnforcedAndReimportIsIdempotent()
    {
        FakeReader reader = new();
        MicrosoftFilesSyncService service = new(reader);
        IntegrationInboxV9Service inbox = new(new IntegrationInboxV9State());
        MicrosoftFilesSelection selection = new() { SelectedDriveIds = ["drive-selected"], MaximumItemsPerSource = 10 };
        DateTimeOffset now = DateTimeOffset.Parse("2026-07-17T02:00:00Z");

        await service.SyncAsync(Account(), selection, inbox, now);
        await service.SyncAsync(Account(), selection, inbox, now.AddMinutes(1));

        Assert.All(reader.RequestedDriveIds, id => Assert.Equal("drive-selected", id));
        Assert.Single(inbox.State.Candidates);
        Assert.Contains(inbox.State.AuditEntries, a => a.Action == IntegrationReviewAuditAction.ReimportIgnored);
    }

    [Fact]
    public async Task PartialFailureIsVisibleAndFailsClosed()
    {
        FakeReader reader = new() { ThrowForDrive = "drive-bad" };
        MicrosoftFilesSyncService service = new(reader);
        IntegrationInboxV9Service inbox = new(new IntegrationInboxV9State());
        MicrosoftProviderAccount account = Account();
        MicrosoftFilesSelection selection = new() { SelectedDriveIds = ["drive-good", "drive-bad"] };

        MicrosoftFilesSyncResult result = await service.SyncAsync(account, selection, inbox, DateTimeOffset.UtcNow);

        Assert.True(result.PartiallySucceeded);
        Assert.Single(result.Errors);
        Assert.Equal("microsoft-files-partial-failure", account.LastErrorCode);
        Assert.Single(inbox.State.Candidates);
    }

    private static MicrosoftProviderAccount Account() => new()
    {
        Id = "microsoft-user-1", DisplayName = "Alex Morgan", RedactedIdentity = "a***@example.invalid",
        Classification = IntegrationAccountClassification.Personal
    };

    private static MicrosoftFileDescriptor Item(string id, string name, MicrosoftFileSourceState state) => new(
        id, name, false, 4096, DateTimeOffset.Parse("2026-07-16T01:00:00Z"),
        DateTimeOffset.Parse("2026-07-17T01:00:00Z"), "https://example.invalid/file",
        "/Projects", "Alex Morgan", "etag-1", "drive-selected", "", "", MicrosoftFileSourceKind.OneDrive, state);

    private sealed class FakeReader : IMicrosoftFilesReader
    {
        public string? ThrowForDrive { get; init; }
        public List<string> RequestedDriveIds { get; } = [];
        public Task<IReadOnlyList<MicrosoftDriveDescriptor>> GetDrivesAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<MicrosoftDriveDescriptor>>([]);
        public Task<IReadOnlyList<MicrosoftSiteDescriptor>> GetSitesAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<MicrosoftSiteDescriptor>>([]);
        public Task<IReadOnlyList<MicrosoftLibraryDescriptor>> GetLibrariesAsync(string siteId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<MicrosoftLibraryDescriptor>>([]);
        public Task<IReadOnlyList<MicrosoftFileDescriptor>> GetDriveItemsAsync(string driveId, string? folderId, DateTimeOffset modifiedSinceUtc, int maximumItems, CancellationToken cancellationToken = default)
        {
            RequestedDriveIds.Add(driveId);
            if (driveId == ThrowForDrive) throw new UnauthorizedAccessException();
            return Task.FromResult<IReadOnlyList<MicrosoftFileDescriptor>>([Item("file-1", "Proposal.pdf", MicrosoftFileSourceState.Current)]);
        }
    }
}
