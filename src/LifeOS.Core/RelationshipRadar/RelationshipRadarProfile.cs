namespace LifeOS.Core.RelationshipRadar;

public sealed class RelationshipRadarProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string RoleOrContext { get; set; } = string.Empty;

    public RelationshipRadarStatus Status { get; set; } = RelationshipRadarStatus.Active;

    public RelationshipWaitingOn WaitingOn { get; set; } = RelationshipWaitingOn.Unknown;

    public DateOnly? LastContactDate { get; set; }

    public DateOnly? NextFollowUpDate { get; set; }

    public string LinkedWork { get; set; } = string.Empty;

    public string NextAction { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public bool DoNotChase { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
