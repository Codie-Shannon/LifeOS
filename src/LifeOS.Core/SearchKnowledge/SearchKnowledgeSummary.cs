namespace LifeOS.Core.SearchKnowledge;

public sealed class SearchKnowledgeSummary
{
    public string Version { get; set; } = string.Empty;

    public string Mode { get; set; } = string.Empty;

    public int TotalItems { get; set; }

    public int ActiveItems { get; set; }

    public int ReviewNeededItems { get; set; }

    public int PlannedItems { get; set; }

    public int ProfileCount { get; set; }

    public int PinnedProfiles { get; set; }

    public int ManualProfiles { get; set; }

    public int SourceCount { get; set; }

    public bool LocalIndexOnly { get; set; }

    public bool ExternalSearchEnabled { get; set; }

    public bool AiReasoningEnabled { get; set; }

    public bool IntegrationSourcesEnabled { get; set; }

    public IReadOnlyList<string> Reasons { get; set; } = [];

    public IReadOnlyList<KnowledgeItem> PriorityItems { get; set; } = [];

    public IReadOnlyList<KnowledgeItem> RecentItems { get; set; } = [];

    public IReadOnlyList<KnowledgeSearchProfile> Profiles { get; set; } = [];
}
