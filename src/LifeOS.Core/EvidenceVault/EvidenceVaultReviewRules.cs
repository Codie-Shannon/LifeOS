namespace LifeOS.Core.EvidenceVault;

public static class EvidenceVaultReviewRules
{
    public static bool IsReviewRequired(EvidenceVaultItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return item.NeedsReview
            || item.Status == EvidenceVaultStatus.NeedsReview
            || string.IsNullOrWhiteSpace(item.SourcePathOrReference)
            || string.IsNullOrWhiteSpace(item.Summary)
            || string.IsNullOrWhiteSpace(item.ProjectOrClient);
    }

    public static string GetReviewReason(EvidenceVaultItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.NeedsReview || item.Status == EvidenceVaultStatus.NeedsReview)
        {
            return "Marked as needing review.";
        }

        if (string.IsNullOrWhiteSpace(item.SourcePathOrReference))
        {
            return "Missing source path/reference.";
        }

        if (string.IsNullOrWhiteSpace(item.ProjectOrClient))
        {
            return "Missing linked project/client context.";
        }

        if (string.IsNullOrWhiteSpace(item.Summary))
        {
            return "Missing summary.";
        }

        return "Review not required.";
    }
}
