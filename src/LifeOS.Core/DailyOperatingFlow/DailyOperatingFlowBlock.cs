namespace LifeOS.Core.DailyOperatingFlow;

public sealed class DailyOperatingFlowBlock
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public string Title { get; set; } = string.Empty;

    public string Area { get; set; } = string.Empty;

    public string Detail { get; set; } = string.Empty;

    public string TimeWindow { get; set; } = string.Empty;

    public DailyOperatingFlowKind Kind { get; set; } = DailyOperatingFlowKind.NextAction;

    public DailyOperatingFlowStatus Status { get; set; } = DailyOperatingFlowStatus.Planned;

    public DailyOperatingFlowPressure Pressure { get; set; } = DailyOperatingFlowPressure.Normal;

    public string NextAction { get; set; } = string.Empty;

    public bool IsPinned { get; set; }

    public bool ShowInToday { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public bool IsOpen => Status is not DailyOperatingFlowStatus.Done and not DailyOperatingFlowStatus.Archived;

    public bool IsTodayVisible(DateOnly today) => Date == today && IsOpen && ShowInToday;
}
