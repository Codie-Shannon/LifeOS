namespace LifeOS.Core.DailyState;

public sealed class ScheduledCommunicationItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Recipient { get; set; } = string.Empty;

    public string Channel { get; set; } = "Email";

    public DateTime ScheduledAt { get; set; } = DateTime.Now;

    public string Purpose { get; set; } = string.Empty;

    public string RelatedProject { get; set; } = string.Empty;

    public ScheduledCommunicationStatus Status { get; set; } = ScheduledCommunicationStatus.Planned;

    public bool FollowUpNeededAfterSend { get; set; }

    public DateOnly? FollowUpDate { get; set; }

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public bool IsOpen => Status is ScheduledCommunicationStatus.Planned or ScheduledCommunicationStatus.WaitingAfterSend;
}
