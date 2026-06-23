namespace LifeOS.Core.Agenda;

public sealed class AgendaItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public AgendaItemType Type { get; set; } = AgendaItemType.Task;

    public AgendaItemStatus Status { get; set; } = AgendaItemStatus.Planned;

    public AgendaPressureLevel PressureLevel { get; set; } = AgendaPressureLevel.Normal;

    public DateOnly? DueDate { get; set; }

    public string TimeText { get; set; } = string.Empty;

    public bool IsFixedCommitment { get; set; }

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
