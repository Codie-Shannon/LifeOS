namespace LifeOS.Core.IntegrationInbox;

public static class Group49IntegrationInboxMigration
{
    public static IntegrationInboxV9State LoadOrCreateProofState(DateTimeOffset nowUtc)
    {
        IntegrationInboxV9State state = IntegrationInboxV9Store.LoadOrCreate(nowUtc);
        IntegrationInboxV9State migrated = Apply(state, nowUtc);

        try
        {
            IntegrationInboxV9Store.Save(migrated);
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
            // The deterministic proof surface remains usable in memory.
        }

        return migrated;
    }

    public static IntegrationInboxV9State Apply(IntegrationInboxV9State state, DateTimeOffset nowUtc)
    {
        AddIfMissing(state, CreateOneDrive(nowUtc));
        AddIfMissing(state, CreateSharePoint(nowUtc));
        AddIfMissing(state, CreateRemoved(nowUtc));
        return state.Normalize();
    }

    private static void AddIfMissing(IntegrationInboxV9State state, IntegrationCandidate candidate)
    {
        if (state.Candidates.All(existing => !string.Equals(existing.Id, candidate.Id, StringComparison.Ordinal))) state.Candidates.Insert(0, candidate);
    }

    private static IntegrationCandidate CreateOneDrive(DateTimeOffset nowUtc)
    {
        IntegrationCandidate c = IntegrationCandidateNormalizer.Normalize(new FileDocumentCandidateDraft
        {
            ProviderId="microsoft", ProviderDisplayName="Microsoft 365", AccountId="microsoft-personal", AccountDisplayName="Codie Shannon · Personal",
            ExternalId="redacted-onedrive-file", CapabilityId="onedrive", RawReference="microsoft://onedrive/selected-folder/file", SourceTimestampUtc=nowUtc.AddMinutes(-18),
            Name="LifeOS Group 49 evidence plan.docx", Extension=".docx", SizeBytes=84216, ModifiedUtc=nowUtc.AddMinutes(-22), WebReference="Open source item in Microsoft 365",
            Summary="OneDrive file metadata from the explicitly selected drive and folder. The body was not downloaded automatically."
        }, nowUtc); c.Id="group49-onedrive-file"; c.Status=IntegrationCandidateStatus.NeedsReview;
        c.Fields.Add(new IntegrationCandidateField{Key="scope",DisplayName="Bounded source",Value="Selected drive · Projects / LifeOS Evidence"});
        c.Fields.Add(new IntegrationCandidateField{Key="owner",DisplayName="Owner",Value="Codie Shannon"});
        c.Fields.Add(new IntegrationCandidateField{Key="etag",DisplayName="Change reference",Value="Redacted ETag · delta tracked"});
        c.Fields.Add(new IntegrationCandidateField{Key="download",DisplayName="File body",Value="Not downloaded"}); return c;
    }

    private static IntegrationCandidate CreateSharePoint(DateTimeOffset nowUtc)
    {
        IntegrationCandidate c = IntegrationCandidateNormalizer.Normalize(new FileDocumentCandidateDraft
        {
            ProviderId="microsoft", ProviderDisplayName="Microsoft 365", AccountId="microsoft-work", AccountDisplayName="Codie Shannon · Work",
            ExternalId="redacted-sharepoint-file", CapabilityId="sharepoint", RawReference="microsoft://sharepoint/selected-library/file", SourceTimestampUtc=nowUtc.AddMinutes(-32),
            Name="LifeOS delivery checklist.xlsx", Extension=".xlsx", SizeBytes=128448, ModifiedUtc=nowUtc.AddMinutes(-40), WebReference="Open source item in SharePoint",
            Summary="SharePoint document metadata from the explicitly selected site and library, reviewed and linked without creating a duplicate editor."
        }, nowUtc); c.Id="group49-sharepoint-linked"; c.Status=IntegrationCandidateStatus.Accepted;
        c.AuthoritativeLink=new LifeOsAuthoritativeLink{Module="Projects",RecordId="project-lifeos-v9",DisplayName="LifeOS v9",LinkedUtc=nowUtc.AddMinutes(-12)};
        c.ReviewNote="Accepted after review and linked to the existing LifeOS v9 Project record.";
        c.Fields.Add(new IntegrationCandidateField{Key="scope",DisplayName="Bounded source",Value="LifeOS Delivery Workspace · Project Documents"});
        c.Fields.Add(new IntegrationCandidateField{Key="link",DisplayName="LifeOS link",Value="Projects · LifeOS v9"}); return c;
    }

    private static IntegrationCandidate CreateRemoved(DateTimeOffset nowUtc)
    {
        IntegrationCandidate c = IntegrationCandidateNormalizer.Normalize(new FileDocumentCandidateDraft
        {
            ProviderId="microsoft", ProviderDisplayName="Microsoft 365", AccountId="microsoft-work", AccountDisplayName="Codie Shannon · Work",
            ExternalId="redacted-removed-file", CapabilityId="sharepoint", RawReference="microsoft://sharepoint/source-removed", SourceTimestampUtc=nowUtc.AddDays(-2),
            Name="Project brief - superseded.docx", Extension=".docx", SizeBytes=46112, ModifiedUtc=nowUtc.AddDays(-3), WebReference="Source no longer available",
            Summary="The source item was removed or access was lost. Provenance is retained and the candidate is not presented as current."
        }, nowUtc); c.Id="group49-source-removed"; c.Status=IntegrationCandidateStatus.SourceRemoved; c.Provenance.IsSourceRemoved=true; c.Provenance.Freshness=IntegrationCandidateFreshness.SourceRemoved;
        c.Fields.Add(new IntegrationCandidateField{Key="recovery",DisplayName="Recovery state",Value="Source removed / permission lost · review required"}); return c;
    }
}
