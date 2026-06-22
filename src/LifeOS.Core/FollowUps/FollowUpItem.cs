namespace LifeOS.Core.FollowUps;

public sealed class FollowUpItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string PersonOrOrganisation { get; set; } = string.Empty;

    public string Context { get; set; } = string.Empty;

    public string NextAction { get; set; } = string.Empty;

    public DateOnly? FollowUpDate { get; set; }

    public FollowUpStatus Status { get; set; } = FollowUpStatus.Waiting;

    public FollowUpPriority Priority { get; set; } = FollowUpPriority.Normal;

    public bool IsMoneyLinked { get; set; }

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}