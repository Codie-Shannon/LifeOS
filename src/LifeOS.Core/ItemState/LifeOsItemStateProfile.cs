namespace LifeOS.Core.ItemState;

public sealed class LifeOsItemStateProfile
{
    public string Version { get; set; } = "v4.1";

    public string Mode { get; set; } = "Item Type / State Engine";

    public string MasterRule { get; set; } = "Everything important becomes an item. Every item has state. Every state affects pressure. Every pressure feeds the Command Centre.";

    public bool RealIntegrationsEnabled { get; set; }

    public bool ItemStateEngineActive { get; set; } = true;

    public bool ManualReviewRequired { get; set; } = true;

    public bool EvidenceBeforeTrustedState { get; set; } = true;

    public List<LifeOsStatefulItem> Items { get; set; } = [];

    public List<LifeOsStateTransitionRule> TransitionRules { get; set; } = [];
}
