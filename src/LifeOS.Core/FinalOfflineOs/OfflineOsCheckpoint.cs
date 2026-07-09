namespace LifeOS.Core.FinalOfflineOs;

public sealed class OfflineOsCheckpoint
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public OfflineOsArea Area { get; set; } = OfflineOsArea.Command;

    public OfflineReadinessStatus Status { get; set; } = OfflineReadinessStatus.Ready;

    public int Priority { get; set; } = 3;

    public bool RequiredForV4 { get; set; }

    public string Evidence { get; set; } = string.Empty;

    public string NextAction { get; set; } = string.Empty;

    public string Boundary { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
