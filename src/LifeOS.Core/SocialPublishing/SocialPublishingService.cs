namespace LifeOS.Core.SocialPublishing;

public enum SocialDestination
{
    FacebookProfile,
    FacebookPage,
    FacebookGroup,
    Messenger
}

public enum SocialDeliveryPath
{
    OfficialApi,
    BrowserAssisted,
    ManualOnly,
    Unsupported
}

public enum SocialDraftState
{
    Draft,
    Approved,
    Cancelled,
    Published
}

public sealed record SocialCapability(
    SocialDestination Destination,
    bool ApiAvailable,
    bool BrowserAssistedAllowed,
    string Limitation);

public sealed record SocialDraft(
    string Id,
    SocialDestination Destination,
    string Target,
    string Body,
    DateTimeOffset ScheduledFor,
    string Source,
    SocialDeliveryPath Path,
    SocialDraftState State,
    DateTimeOffset? ApprovedAt);

public sealed class SocialPublishingService
{
    public SocialDeliveryPath ResolvePath(SocialCapability capability)
    {
        if (capability.ApiAvailable)
        {
            return SocialDeliveryPath.OfficialApi;
        }
        if (capability.BrowserAssistedAllowed)
        {
            return SocialDeliveryPath.BrowserAssisted;
        }
        return string.IsNullOrWhiteSpace(capability.Limitation)
            ? SocialDeliveryPath.ManualOnly
            : SocialDeliveryPath.Unsupported;
    }

    public SocialDraft Draft(
        SocialCapability capability,
        string target,
        string body,
        DateTimeOffset scheduledFor,
        string source)
    {
        if (string.IsNullOrWhiteSpace(target) || string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("A target and body are required.");
        }

        return new SocialDraft(
            Guid.NewGuid().ToString("N"),
            capability.Destination,
            target.Trim(),
            body.Trim(),
            scheduledFor,
            source.Trim(),
            ResolvePath(capability),
            SocialDraftState.Draft,
            null);
    }

    public SocialDraft Approve(SocialDraft draft, DateTimeOffset at) =>
        draft.Path == SocialDeliveryPath.Unsupported
            ? throw new InvalidOperationException("This platform path is unsupported.")
            : draft.State != SocialDraftState.Draft
                ? throw new InvalidOperationException("Only a draft can be approved.")
                : draft with { State = SocialDraftState.Approved, ApprovedAt = at };

    public SocialDraft Edit(SocialDraft draft, string body) =>
        draft.State is SocialDraftState.Published or SocialDraftState.Cancelled
            ? throw new InvalidOperationException("Published or cancelled drafts cannot be edited.")
            : draft with
            {
                Body = body.Trim(),
                State = SocialDraftState.Draft,
                ApprovedAt = null
            };

    public SocialDraft Cancel(SocialDraft draft) =>
        draft.State == SocialDraftState.Published
            ? throw new InvalidOperationException("Published content cannot be cancelled.")
            : draft with { State = SocialDraftState.Cancelled };

    public SocialDraft MarkPublished(SocialDraft draft, bool providerConfirmed) =>
        draft.State == SocialDraftState.Approved && providerConfirmed
            ? draft with { State = SocialDraftState.Published }
            : throw new InvalidOperationException("Approval and provider confirmation are required.");
}
