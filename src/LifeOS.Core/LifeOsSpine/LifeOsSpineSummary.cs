namespace LifeOS.Core.LifeOsSpine;

public sealed class LifeOsSpineSummary
{
    public string Version { get; set; } = string.Empty;

    public string Mode { get; set; } = string.Empty;

    public string MasterRule { get; set; } = string.Empty;

    public int ModuleCount { get; set; }

    public int CanonModules { get; set; }

    public int ActiveModules { get; set; }

    public int NeedsModelModules { get; set; }

    public int RequiredForV4Modules { get; set; }

    public int ItemRuleCount { get; set; }

    public int ItemTypesCovered { get; set; }

    public int StateCount { get; set; }

    public int PressureSourceCount { get; set; }

    public int CriticalPressureSources { get; set; }

    public bool IntegrationsDeferredToV5 { get; set; }

    public bool CompanionAppDeferredToV65 { get; set; }

    public bool MajorUiReshapeDeferred { get; set; }

    public bool ItemStateModelRequired { get; set; }

    public bool WeeklyPressureEngineRequired { get; set; }

    public IReadOnlyList<string> Reasons { get; set; } = [];

    public IReadOnlyList<LifeOsSpineModule> CoreModules { get; set; } = [];

    public IReadOnlyList<LifeOsStatefulItemRule> ItemRules { get; set; } = [];

    public IReadOnlyList<LifeOsPressureSource> PressureSources { get; set; } = [];
}
