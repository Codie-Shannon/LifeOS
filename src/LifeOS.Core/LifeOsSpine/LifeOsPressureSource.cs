namespace LifeOS.Core.LifeOsSpine;

public sealed class LifeOsPressureSource
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public LifeOsSpineArea Area { get; set; } = LifeOsSpineArea.PressureEngine;

    public PressureImpactLevel ImpactLevel { get; set; } = PressureImpactLevel.Medium;

    public string SourceItems { get; set; } = string.Empty;

    public string PressureQuestion { get; set; } = string.Empty;

    public string CommandCentreSignal { get; set; } = string.Empty;

    public string SafeNextAction { get; set; } = string.Empty;
}
