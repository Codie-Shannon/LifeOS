namespace LifeOS.Core.UniversalSpine;

public sealed class UniversalSpineProfile
{
    public string Version { get; set; } = "v2.1";

    public string SpineMode { get; set; } = "Local universal spine";

    public bool UniversalSearchPlanned { get; set; } = true;

    public bool CrossModuleLinksEnabled { get; set; } = true;

    public bool ManualReviewRequired { get; set; } = true;

    public bool ExternalSyncEnabled { get; set; }

    public string Notes { get; set; } = "Universal spine foundation for cross-module context and links.";

    public List<UniversalSpineItem> Items { get; set; } = [];

    public List<UniversalSpineLink> Links { get; set; } = [];
}
