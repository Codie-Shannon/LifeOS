using LifeOS.Modules.Timer.Models;

namespace LifeOS.Modules.Timer.Storage;

public sealed class TimerLogEntry
{
    public Guid Id { get; init; }
    public DateOnly Date { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }

    public double DurationMinutes { get; init; }
    public string ClientName { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public string WorkType { get; init; } = string.Empty;
    public bool IsBillable { get; init; }

    public decimal HourlyRate { get; init; }
    public decimal EarnedAmount { get; init; }
    public decimal TaxSetAsidePercent { get; init; }
    public decimal TaxSetAsideAmount { get; init; }
    public decimal SafeAfterTaxAmount { get; init; }

    public string Notes { get; init; } = string.Empty;

    public static TimerLogEntry FromSession(TimerSession session)
    {
        var endedAt = session.EndedAt ?? DateTimeOffset.Now;
        var duration = session.AccumulatedDuration;

        var earnedAmount = session.GetEarnedAmount();
        var taxSetAsideAmount = session.GetTaxSetAside();

        return new TimerLogEntry
        {
            Id = session.Id,
            Date = DateOnly.FromDateTime(session.StartedAt.LocalDateTime),
            StartTime = TimeOnly.FromDateTime(session.StartedAt.LocalDateTime),
            EndTime = TimeOnly.FromDateTime(endedAt.LocalDateTime),
            DurationMinutes = Math.Round(duration.TotalMinutes, 2),

            ClientName = session.ClientName,
            ProjectName = session.ProjectName,
            WorkType = session.WorkType,
            IsBillable = session.IsBillable,

            HourlyRate = session.HourlyRate,
            EarnedAmount = earnedAmount,
            TaxSetAsidePercent = session.TaxSetAsidePercent,
            TaxSetAsideAmount = taxSetAsideAmount,
            SafeAfterTaxAmount = earnedAmount - taxSetAsideAmount,

            Notes = session.Notes
        };
    }
}