namespace LifeOS.Modules.Timer.Models;

public sealed class TimedTask
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public Guid? ContactId { get; set; }
    public string ContactName { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;
    public string WorkType { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public TimedTaskType TaskType { get; set; } = TimedTaskType.Work;
    public TimedTaskMode Mode { get; set; } = TimedTaskMode.Exclusive;
    public Guid? LinkedTaskId { get; set; }

    public bool IsBillable { get; set; } = true;
    public decimal HourlyRate { get; set; } = 35m;
    public decimal TaxSetAsidePercent { get; set; } = 20m;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;

    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? LastStartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }

    public TimeSpan AccumulatedDuration { get; set; } = TimeSpan.Zero;
    public TimedTaskState State { get; set; } = TimedTaskState.Stopped;

    public bool IsArchived { get; set; }

    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                return Name.Trim();
            }

            if (!string.IsNullOrWhiteSpace(ContactName) && !string.IsNullOrWhiteSpace(ProjectName))
            {
                return $"{ContactName.Trim()} · {ProjectName.Trim()}";
            }

            if (!string.IsNullOrWhiteSpace(ContactName))
            {
                return ContactName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(ProjectName))
            {
                return ProjectName.Trim();
            }

            return "Untitled task";
        }
    }

    public TimeSpan GetCurrentDuration(DateTimeOffset? now = null)
    {
        if (State != TimedTaskState.Running || LastStartedAt is null)
        {
            return AccumulatedDuration;
        }

        var currentTime = now ?? DateTimeOffset.Now;
        return AccumulatedDuration + (currentTime - LastStartedAt.Value);
    }

    public decimal GetEarnedAmount(DateTimeOffset? now = null)
    {
        if (!IsBillable)
        {
            return 0m;
        }

        var hours = (decimal)GetCurrentDuration(now).TotalHours;
        return Math.Round(hours * HourlyRate, 2);
    }

    public decimal GetTaxSetAside(DateTimeOffset? now = null)
    {
        var earned = GetEarnedAmount(now);
        return Math.Round(earned * (TaxSetAsidePercent / 100m), 2);
    }

    public decimal GetSafeAfterTax(DateTimeOffset? now = null)
    {
        return GetEarnedAmount(now) - GetTaxSetAside(now);
    }

    public TimedTask CreateSnapshot(DateTimeOffset? endedAt = null)
    {
        var endTime = endedAt ?? DateTimeOffset.Now;

        return new TimedTask
        {
            Id = Id,
            Name = Name,
            ContactId = ContactId,
            ContactName = ContactName,
            ProjectName = ProjectName,
            WorkType = WorkType,
            Notes = Notes,
            TaskType = TaskType,
            Mode = Mode,
            LinkedTaskId = LinkedTaskId,
            IsBillable = IsBillable,
            HourlyRate = HourlyRate,
            TaxSetAsidePercent = TaxSetAsidePercent,
            StartedAt = StartedAt,
            LastStartedAt = null,
            EndedAt = endTime,
            AccumulatedDuration = GetCurrentDuration(endTime),
            State = TimedTaskState.Done,
            IsArchived = IsArchived
        };
    }

    public void ResetAfterSave(DateTimeOffset? now = null)
    {
        var currentTime = now ?? DateTimeOffset.Now;

        StartedAt = currentTime;
        LastStartedAt = null;
        EndedAt = null;
        AccumulatedDuration = TimeSpan.Zero;
        State = TimedTaskState.Stopped;
    }
}