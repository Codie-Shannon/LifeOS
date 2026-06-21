using LifeOS.Modules.Timer.Models;

namespace LifeOS.Modules.Timer.Storage;

public sealed class TimerLogEntry
{
    public Guid Id { get; init; }
    public Guid TimedTaskId { get; init; }

    public string TimedTaskName { get; init; } = string.Empty;
    public string TaskType { get; init; } = string.Empty;
    public string TaskMode { get; init; } = string.Empty;

    public DateOnly Date { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }

    public double DurationMinutes { get; init; }

    public Guid? ContactId { get; init; }
    public string ContactName { get; init; } = string.Empty;

    public string ProjectName { get; init; } = string.Empty;
    public string WorkType { get; init; } = string.Empty;
    public bool IsBillable { get; init; }

    public decimal HourlyRate { get; init; }
    public decimal EarnedAmount { get; init; }
    public decimal TaxSetAsidePercent { get; init; }
    public decimal TaxSetAsideAmount { get; init; }
    public decimal SafeAfterTaxAmount { get; init; }

    public string Notes { get; init; } = string.Empty;

    public static TimerLogEntry FromTimedTask(TimedTask task)
    {
        var endedAt = task.EndedAt ?? DateTimeOffset.Now;
        var duration = task.GetCurrentDuration(endedAt);

        var earnedAmount = task.GetEarnedAmount(endedAt);
        var taxSetAsideAmount = task.GetTaxSetAside(endedAt);

        return new TimerLogEntry
        {
            Id = Guid.NewGuid(),
            TimedTaskId = task.Id,
            TimedTaskName = task.DisplayName,
            TaskType = task.TaskType.ToString(),
            TaskMode = task.Mode.ToString(),

            Date = DateOnly.FromDateTime(task.StartedAt.LocalDateTime),
            StartTime = TimeOnly.FromDateTime(task.StartedAt.LocalDateTime),
            EndTime = TimeOnly.FromDateTime(endedAt.LocalDateTime),
            DurationMinutes = Math.Round(duration.TotalMinutes, 2),

            ContactId = task.ContactId,
            ContactName = task.ContactName,

            ProjectName = task.ProjectName,
            WorkType = task.WorkType,
            IsBillable = task.IsBillable,

            HourlyRate = task.HourlyRate,
            EarnedAmount = earnedAmount,
            TaxSetAsidePercent = task.TaxSetAsidePercent,
            TaxSetAsideAmount = taxSetAsideAmount,
            SafeAfterTaxAmount = earnedAmount - taxSetAsideAmount,

            Notes = task.Notes
        };
    }
}