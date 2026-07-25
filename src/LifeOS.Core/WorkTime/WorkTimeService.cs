using System.Globalization;
using System.Text;

namespace LifeOS.Core.WorkTime;

public enum WorkTimerState
{
    Running,
    Paused,
    Completed
}

public sealed record WorkEvidence(
    string Source,
    DateTimeOffset CapturedAt,
    string Reference,
    string Note);

public sealed record WorkTimeEntry(
    Guid Id,
    string Client,
    string Project,
    string Description,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    TimeSpan Accumulated,
    DateTimeOffset? SegmentStartedAt,
    WorkTimerState State,
    bool IsBillable,
    decimal HourlyRate,
    IReadOnlyList<WorkEvidence> Evidence)
{
    public TimeSpan Duration(DateTimeOffset now) =>
        State == WorkTimerState.Running && SegmentStartedAt is not null
            ? Accumulated + (now - SegmentStartedAt.Value)
            : Accumulated;

    public decimal BillableValue(DateTimeOffset now) =>
        IsBillable
            ? Math.Round((decimal)Duration(now).TotalHours * HourlyRate, 2)
            : 0m;
}

public sealed record WorkDaySummary(
    DateOnly Date,
    TimeSpan Total,
    TimeSpan Billable,
    TimeSpan NonBillable,
    decimal BillableValue,
    int EvidenceCount);

public sealed class WorkTimeService
{
    public WorkTimeEntry Start(
        string client,
        string project,
        string description,
        bool isBillable,
        decimal hourlyRate,
        DateTimeOffset now)
    {
        ValidateText(project, nameof(project));
        ValidateText(description, nameof(description));
        if (hourlyRate < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(hourlyRate));
        }

        return new WorkTimeEntry(
            Guid.NewGuid(),
            client.Trim(),
            project.Trim(),
            description.Trim(),
            now,
            null,
            TimeSpan.Zero,
            now,
            WorkTimerState.Running,
            isBillable,
            hourlyRate,
            new[]
            {
                new WorkEvidence("local-timer", now, "timer-start", "Timer started explicitly by the user.")
            });
    }

    public WorkTimeEntry Pause(WorkTimeEntry entry, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (entry.State != WorkTimerState.Running || entry.SegmentStartedAt is null)
        {
            throw new InvalidOperationException("Only a running timer can be paused.");
        }

        return entry with
        {
            Accumulated = entry.Accumulated + (now - entry.SegmentStartedAt.Value),
            SegmentStartedAt = null,
            State = WorkTimerState.Paused,
            Evidence = Append(entry, now, "timer-pause", "Timer paused explicitly by the user.")
        };
    }

    public WorkTimeEntry Resume(WorkTimeEntry entry, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (entry.State != WorkTimerState.Paused)
        {
            throw new InvalidOperationException("Only a paused timer can be resumed.");
        }

        return entry with
        {
            SegmentStartedAt = now,
            State = WorkTimerState.Running,
            Evidence = Append(entry, now, "timer-resume", "Timer resumed explicitly by the user.")
        };
    }

    public WorkTimeEntry Stop(WorkTimeEntry entry, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (entry.State == WorkTimerState.Completed)
        {
            throw new InvalidOperationException("The timer is already complete.");
        }

        TimeSpan duration = entry.Duration(now);
        if (duration <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("A work entry must contain positive time.");
        }

        return entry with
        {
            Accumulated = duration,
            SegmentStartedAt = null,
            EndedAt = now,
            State = WorkTimerState.Completed,
            Evidence = Append(entry, now, "timer-stop", "Timer stopped and retained as local source proof.")
        };
    }

    public WorkTimeEntry AddManual(
        string client,
        string project,
        string description,
        DateTimeOffset startedAt,
        DateTimeOffset endedAt,
        bool isBillable,
        decimal hourlyRate,
        string reason,
        DateTimeOffset recordedAt)
    {
        ValidateText(project, nameof(project));
        ValidateText(description, nameof(description));
        ValidateText(reason, nameof(reason));
        if (endedAt <= startedAt)
        {
            throw new ArgumentException("Manual time must end after it starts.", nameof(endedAt));
        }

        return new WorkTimeEntry(
            Guid.NewGuid(),
            client.Trim(),
            project.Trim(),
            description.Trim(),
            startedAt,
            endedAt,
            endedAt - startedAt,
            null,
            WorkTimerState.Completed,
            isBillable,
            hourlyRate,
            new[]
            {
                new WorkEvidence("manual-entry", recordedAt, "manual-time", reason.Trim())
            });
    }

    public WorkTimeEntry AttachEvidence(
        WorkTimeEntry entry,
        string source,
        string reference,
        string note,
        DateTimeOffset capturedAt)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ValidateText(source, nameof(source));
        ValidateText(reference, nameof(reference));

        return entry with
        {
            Evidence = entry.Evidence
                .Append(new WorkEvidence(source.Trim(), capturedAt, reference.Trim(), note.Trim()))
                .ToArray()
        };
    }

    public WorkDaySummary Summarize(
        IEnumerable<WorkTimeEntry> entries,
        DateOnly date,
        DateTimeOffset now)
    {
        WorkTimeEntry[] selected = entries
            .Where(entry => DateOnly.FromDateTime(entry.StartedAt.LocalDateTime) == date)
            .ToArray();

        TimeSpan total = TimeSpan.FromTicks(selected.Sum(entry => entry.Duration(now).Ticks));
        TimeSpan billable = TimeSpan.FromTicks(selected
            .Where(entry => entry.IsBillable)
            .Sum(entry => entry.Duration(now).Ticks));

        return new WorkDaySummary(
            date,
            total,
            billable,
            total - billable,
            selected.Sum(entry => entry.BillableValue(now)),
            selected.Sum(entry => entry.Evidence.Count));
    }

    public string ExportTimesheet(
        IEnumerable<WorkTimeEntry> entries,
        DateTimeOffset now)
    {
        StringBuilder csv = new();
        csv.AppendLine("Date,Client,Project,Description,Hours,Billable,HourlyRate,Value,Evidence");
        foreach (WorkTimeEntry entry in entries.OrderBy(entry => entry.StartedAt))
        {
            csv.AppendLine(string.Join(
                ',',
                Csv(entry.StartedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                Csv(entry.Client),
                Csv(entry.Project),
                Csv(entry.Description),
                entry.Duration(now).TotalHours.ToString("0.00", CultureInfo.InvariantCulture),
                entry.IsBillable.ToString(CultureInfo.InvariantCulture),
                entry.HourlyRate.ToString("0.00", CultureInfo.InvariantCulture),
                entry.BillableValue(now).ToString("0.00", CultureInfo.InvariantCulture),
                entry.Evidence.Count.ToString(CultureInfo.InvariantCulture)));
        }

        return csv.ToString();
    }

    private static IReadOnlyList<WorkEvidence> Append(
        WorkTimeEntry entry,
        DateTimeOffset at,
        string reference,
        string note) =>
        entry.Evidence
            .Append(new WorkEvidence("local-timer", at, reference, note))
            .ToArray();

    private static void ValidateText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A value is required.", parameterName);
        }
    }

    private static string Csv(string value) =>
        $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
