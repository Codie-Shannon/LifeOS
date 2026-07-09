namespace LifeOS.Core.UniversalSpine;

public sealed class UniversalSpineSummary
{
    public string Version { get; set; } = string.Empty;

    public string SpineMode { get; set; } = string.Empty;

    public int TotalItems { get; set; }

    public int ActiveItems { get; set; }

    public int ReviewNeededItems { get; set; }

    public int BlockedItems { get; set; }

    public int WaitingItems { get; set; }

    public int LinkCount { get; set; }

    public int NeedsProofLinks { get; set; }

    public int FollowUpLinks { get; set; }

    public int MoneyLinks { get; set; }

    public int ModuleCount { get; set; }

    public bool ExternalSyncEnabled { get; set; }

    public IReadOnlyList<string> Reasons { get; set; } = [];

    public IReadOnlyList<UniversalSpineItem> PriorityItems { get; set; } = [];

    public IReadOnlyList<UniversalSpineItem> RecentItems { get; set; } = [];

    public IReadOnlyList<UniversalSpineLink> Links { get; set; } = [];
}
