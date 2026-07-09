namespace LifeOS.Core.LifeOsSpine;

public sealed class LifeOsSpineModule
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public LifeOsSpineArea Area { get; set; } = LifeOsSpineArea.CommandCentre;

    public LifeOsSpineStatus Status { get; set; } = LifeOsSpineStatus.Canon;

    public int Priority { get; set; } = 3;

    public bool RequiredForV4 { get; set; }

    public string Purpose { get; set; } = string.Empty;

    public string ConnectsTo { get; set; } = string.Empty;

    public string CommandCentreSignal { get; set; } = string.Empty;

    public string Boundary { get; set; } = string.Empty;
}
