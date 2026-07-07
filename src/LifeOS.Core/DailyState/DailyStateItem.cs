namespace LifeOS.Core.DailyState;

public sealed class DailyStateItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public string Title { get; set; } = string.Empty;

    public string Detail { get; set; } = string.Empty;

    public string RelatedProject { get; set; } = string.Empty;

    public DailyStateType Type { get; set; } = DailyStateType.NextAction;

    public DailyStateStatus Status { get; set; } = DailyStateStatus.Open;

    public string NextAction { get; set; } = string.Empty;

    public bool ShowInToday { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public bool IsOpen => Status is not DailyStateStatus.Done and not DailyStateStatus.Archived;
}
