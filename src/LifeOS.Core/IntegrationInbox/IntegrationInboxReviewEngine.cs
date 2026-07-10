namespace LifeOS.Core.IntegrationInbox;

public static class IntegrationInboxReviewEngine
{
    public static bool CanAccept(IntegrationPreviewItem item, out string reason)
    {
        if (string.IsNullOrWhiteSpace(item.SourceEvidence))
        {
            reason = "Source evidence is required before acceptance.";
            return false;
        }

        if (item.Status == IntegrationPreviewStatus.DuplicateSuspected)
        {
            reason = "Resolve the duplicate suspicion before acceptance.";
            return false;
        }

        if (item.TrustState == IntegrationTrustState.Untrusted)
        {
            reason = "Review and source-back the preview before acceptance.";
            return false;
        }

        reason = "Preview can be accepted for a later manual handoff.";
        return true;
    }

    public static void Accept(IntegrationPreviewItem item, string reviewNote)
    {
        if (!CanAccept(item, out var reason))
        {
            throw new InvalidOperationException(reason);
        }

        item.Status = IntegrationPreviewStatus.Accepted;
        item.TrustState = IntegrationTrustState.Reviewed;
        item.ReviewNote = reviewNote;
        item.UpdatedAt = DateTime.Now;
    }

    public static void Reject(IntegrationPreviewItem item, string reviewNote)
    {
        item.Status = IntegrationPreviewStatus.Rejected;
        item.ReviewNote = reviewNote;
        item.UpdatedAt = DateTime.Now;
    }

    public static void Defer(IntegrationPreviewItem item, string reviewNote)
    {
        item.Status = IntegrationPreviewStatus.Deferred;
        item.ReviewNote = reviewNote;
        item.UpdatedAt = DateTime.Now;
    }

    public static void Link(IntegrationPreviewItem item, string linkReference)
    {
        if (item.Status != IntegrationPreviewStatus.Accepted)
        {
            throw new InvalidOperationException("Only accepted previews can be linked.");
        }

        item.Status = IntegrationPreviewStatus.Linked;
        item.LinkReference = linkReference;
        item.TrustState = IntegrationTrustState.Trusted;
        item.UpdatedAt = DateTime.Now;
    }
}
