namespace LifeOS.Core.DesktopRelease;

public sealed class DesktopReleaseReadinessSummary
{
    public string Version { get; set; } = string.Empty;

    public string ReleaseLane { get; set; } = string.Empty;

    public string ReleaseStateLabel { get; set; } = string.Empty;

    public int TotalChecks { get; set; }

    public int CompleteChecks { get; set; }

    public int ReviewNeededChecks { get; set; }

    public int PlannedNextChecks { get; set; }

    public int BlockedChecks { get; set; }

    public int ScorePercent { get; set; }

    public bool IsLocalOnly { get; set; }

    public bool IsManualReviewProtected { get; set; }

    public bool IsExpectedMoneyProtected { get; set; }

    public bool IsScreenshotSafe { get; set; }

    public IReadOnlyList<string> Reasons { get; set; } = [];

    public IReadOnlyList<DesktopReleaseChecklistItem> PriorityItems { get; set; } = [];

    public IReadOnlyList<DesktopReleaseChecklistItem> CompletedItems { get; set; } = [];

    public IReadOnlyList<DesktopReleaseChecklistItem> PlannedNextItems { get; set; } = [];
}
