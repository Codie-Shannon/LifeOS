namespace LifeOS.Core.FinalOfflineOs;

public sealed class IntegrationLandingZone
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string SourceName { get; set; } = string.Empty;

    public IntegrationLandingZoneType ZoneType { get; set; } = IntegrationLandingZoneType.ManualImport;

    public string TargetModule { get; set; } = string.Empty;

    public string SpineConnection { get; set; } = string.Empty;

    public OfflineReadinessStatus Status { get; set; } = OfflineReadinessStatus.PlannedForV4;

    public string SafetyRule { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
