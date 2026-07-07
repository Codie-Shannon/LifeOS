namespace LifeOS.Core.EvidenceVault;

public static class EvidenceVaultCalculator
{
    public static EvidenceVaultSummary Calculate(IEnumerable<EvidenceVaultItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var itemList = items.ToList();
        var openItems = itemList.Where(item => item.IsOpen).ToList();
        var needsReview = openItems
            .Where(item => item.NeedsReview || item.Status == EvidenceVaultStatus.NeedsReview)
            .OrderByDescending(item => item.EvidenceDate)
            .ThenBy(item => item.Title)
            .ToList();

        var reasons = new List<string>();
        if (needsReview.Count > 0) reasons.Add($"{needsReview.Count} evidence item(s) need review.");
        if (openItems.Count > 0) reasons.Add($"{openItems.Count} evidence item(s) are open in the vault.");
        if (reasons.Count == 0) reasons.Add("Evidence Vault is ready for metadata records. No evidence pressure detected.");

        return new EvidenceVaultSummary
        {
            TotalItems = itemList.Count,
            OpenItems = openItems.Count,
            NeedsReviewCount = needsReview.Count,
            ScreenshotCount = openItems.Count(item => item.Type == EvidenceVaultItemType.Screenshot),
            EmailProofCount = openItems.Count(item => item.Type == EvidenceVaultItemType.EmailMessage),
            TimesheetProofCount = openItems.Count(item => item.Type == EvidenceVaultItemType.TimesheetDescription || item.Type == EvidenceVaultItemType.WorkSessionNote),
            CommitProofCount = openItems.Count(item => item.Type == EvidenceVaultItemType.CodeCommit),
            NeedsReviewItems = needsReview,
            Reasons = reasons
        };
    }
}
