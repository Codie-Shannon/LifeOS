namespace LifeOS.Core.WeeklyCloseOut;

public sealed class WeeklyCloseOutEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateOnly WeekStart { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public string WhatGotDone { get; set; } = string.Empty;

    public string WhatMoved { get; set; } = string.Empty;

    public string StillWaitingOn { get; set; } = string.Empty;

    public string NextWeekPressure { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
