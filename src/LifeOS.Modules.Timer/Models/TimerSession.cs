namespace LifeOS.Modules.Timer.Models;

public sealed class TimerSession
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string ClientName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string WorkType { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public bool IsBillable { get; set; } = true;
    public decimal HourlyRate { get; set; } = 35m;
    public decimal TaxSetAsidePercent { get; set; } = 20m;

    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.Now;
    public DateTimeOffset? EndedAt { get; set; }

    public TimeSpan AccumulatedDuration { get; set; } = TimeSpan.Zero;
    public TimerState State { get; set; } = TimerState.Stopped;

    public decimal GetEarnedAmount()
    {
        if (!IsBillable)
        {
            return 0m;
        }

        var hours = (decimal)AccumulatedDuration.TotalHours;
        return Math.Round(hours * HourlyRate, 2);
    }

    public decimal GetTaxSetAside()
    {
        var earned = GetEarnedAmount();
        return Math.Round(earned * (TaxSetAsidePercent / 100m), 2);
    }

    public decimal GetSafeAfterTax()
    {
        return GetEarnedAmount() - GetTaxSetAside();
    }
}