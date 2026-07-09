namespace LifeOS.Core.ItemState;

public sealed class LifeOsItemStateSummary
{
    public string Version { get; set; } = string.Empty;

    public string Mode { get; set; } = string.Empty;

    public string MasterRule { get; set; } = string.Empty;

    public int TotalItems { get; set; }

    public int OpenItems { get; set; }

    public int NeedsReviewItems { get; set; }

    public int DueSoonOrTodayItems { get; set; }

    public int OverdueItems { get; set; }

    public int WaitingItems { get; set; }

    public int PaidOrClosedItems { get; set; }

    public int UntrustedItems { get; set; }

    public int MoneyImpactItems { get; set; }

    public int SafeToSpendImpactItems { get; set; }

    public int AgendaImpactItems { get; set; }

    public int WeeklyCloseOutImpactItems { get; set; }

    public int CommandCentreImpactItems { get; set; }

    public int TransitionRules { get; set; }

    public int EvidenceRequiredTransitions { get; set; }

    public int ManualReviewTransitions { get; set; }

    public bool RealIntegrationsEnabled { get; set; }

    public bool ItemStateEngineActive { get; set; }

    public bool ManualReviewRequired { get; set; }

    public bool EvidenceBeforeTrustedState { get; set; }

    public IReadOnlyList<string> Reasons { get; set; } = [];

    public IReadOnlyList<LifeOsStatefulItem> ReviewQueue { get; set; } = [];

    public IReadOnlyList<LifeOsStatefulItem> PressureItems { get; set; } = [];

    public IReadOnlyList<LifeOsStatefulItem> MoneyItems { get; set; } = [];

    public IReadOnlyList<LifeOsStatefulItem> WorkItems { get; set; } = [];

    public IReadOnlyList<LifeOsStateTransitionRule> Rules { get; set; } = [];
}
