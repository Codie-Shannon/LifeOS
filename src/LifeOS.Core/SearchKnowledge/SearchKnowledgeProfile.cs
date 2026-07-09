namespace LifeOS.Core.SearchKnowledge;

public sealed class SearchKnowledgeProfile
{
    public string Version { get; set; } = "v3.5";

    public string Mode { get; set; } = "Local advanced search and knowledge foundation";

    public bool LocalIndexOnly { get; set; } = true;

    public bool ManualReviewRequired { get; set; } = true;

    public bool ExternalSearchEnabled { get; set; }

    public bool AiReasoningEnabled { get; set; }

    public bool IntegrationSourcesEnabled { get; set; }

    public string Notes { get; set; } = "Local knowledge and search profile foundation before integrations and AI assistant layers.";

    public List<KnowledgeItem> Items { get; set; } = [];

    public List<KnowledgeSearchProfile> SearchProfiles { get; set; } = [];
}
