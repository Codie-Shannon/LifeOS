namespace LifeOS.Core.EvidenceVault;

public sealed class EvidenceVaultSummary
{
    public int TotalItems { get; init; }

    public int OpenItems { get; init; }

    public int NeedsReviewCount { get; init; }

    public int ScreenshotCount { get; init; }

    public int EmailProofCount { get; init; }

    public int TimesheetProofCount { get; init; }

    public int CommitProofCount { get; init; }

    public IReadOnlyList<EvidenceVaultItem> NeedsReviewItems { get; init; } = [];

    public IReadOnlyList<string> Reasons { get; init; } = [];
}
