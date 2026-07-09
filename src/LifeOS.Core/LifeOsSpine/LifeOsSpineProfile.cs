namespace LifeOS.Core.LifeOsSpine;

public sealed class LifeOsSpineProfile
{
    public string Version { get; set; } = "v4.0";

    public string Mode { get; set; } = "LifeOS spine recovery map";

    public string MasterRule { get; set; } = "Everything important becomes an item. Every item has state. Every state affects pressure. Every pressure feeds the Command Centre.";

    public bool IntegrationsDeferredToV5 { get; set; } = true;

    public bool CompanionAppDeferredToV65 { get; set; } = true;

    public bool MajorUiReshapeDeferred { get; set; } = true;

    public bool ItemStateModelRequired { get; set; } = true;

    public bool WeeklyPressureEngineRequired { get; set; } = true;

    public List<LifeOsSpineModule> Modules { get; set; } = [];

    public List<LifeOsStatefulItemRule> ItemRules { get; set; } = [];

    public List<LifeOsPressureSource> PressureSources { get; set; } = [];
}
