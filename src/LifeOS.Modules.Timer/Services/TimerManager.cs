using LifeOS.Modules.Timer.Models;

namespace LifeOS.Modules.Timer.Services;

public sealed class TimerManager
{
    private readonly List<TimedTask> _tasks = new();

    public IReadOnlyList<TimedTask> Tasks => _tasks
        .Where(task => !task.IsArchived)
        .ToList();

    public TimedTask? SelectedTask { get; private set; }

    public IReadOnlyList<TimedTask> RunningTasks => _tasks
        .Where(task => task.State == TimedTaskState.Running && !task.IsArchived)
        .ToList();

    public TimedTask? ActiveExclusiveTask => _tasks
        .FirstOrDefault(task =>
            task.State == TimedTaskState.Running &&
            task.Mode == TimedTaskMode.Exclusive &&
            !task.IsArchived);

    public TimedTask CreateTask(
        string name,
        Guid? contactId,
        string contactName,
        string projectName,
        string workType,
        TimedTaskType taskType,
        TimedTaskMode mode,
        bool isBillable,
        decimal hourlyRate,
        decimal taxSetAsidePercent,
        string notes,
        Guid? linkedTaskId = null)
    {
        var now = DateTimeOffset.Now;

        var task = new TimedTask
        {
            Name = name.Trim(),
            ContactId = contactId,
            ContactName = contactName.Trim(),
            ProjectName = projectName.Trim(),
            WorkType = workType.Trim(),
            TaskType = taskType,
            Mode = mode,
            LinkedTaskId = linkedTaskId,
            IsBillable = isBillable,
            HourlyRate = hourlyRate,
            TaxSetAsidePercent = taxSetAsidePercent,
            Notes = notes.Trim(),
            StartedAt = now,
            State = TimedTaskState.Stopped
        };

        _tasks.Add(task);
        SelectedTask = task;

        return task;
    }

    public bool SelectTask(Guid taskId)
    {
        var task = FindTask(taskId);

        if (task is null)
        {
            return false;
        }

        SelectedTask = task;
        return true;
    }

    public bool StartTask(Guid taskId)
    {
        var task = FindTask(taskId);

        if (task is null)
        {
            return false;
        }

        if (task.Mode == TimedTaskMode.Exclusive)
        {
            PauseOtherExclusiveTasks(task.Id);
        }

        StartOrResume(task);
        SelectedTask = task;

        return true;
    }

    public bool PauseTask(Guid taskId)
    {
        var task = FindTask(taskId);

        if (task is null || task.State != TimedTaskState.Running)
        {
            return false;
        }

        Pause(task);
        return true;
    }

    public bool ResumeTask(Guid taskId)
    {
        var task = FindTask(taskId);

        if (task is null || task.State != TimedTaskState.Paused)
        {
            return false;
        }

        if (task.Mode == TimedTaskMode.Exclusive)
        {
            PauseOtherExclusiveTasks(task.Id);
        }

        StartOrResume(task);
        SelectedTask = task;

        return true;
    }

    public TimedTask? StopTask(Guid taskId, bool resetAfterSave = true)
    {
        var task = FindTask(taskId);

        if (task is null)
        {
            return null;
        }

        var now = DateTimeOffset.Now;

        if (task.State == TimedTaskState.Running)
        {
            Pause(task);
        }

        task.EndedAt = now;
        task.State = TimedTaskState.Done;

        var completedSnapshot = task.CreateSnapshot(now);

        if (resetAfterSave)
        {
            task.ResetAfterSave(now);
        }

        return completedSnapshot;
    }

    public bool ArchiveTask(Guid taskId)
    {
        var task = FindTask(taskId);

        if (task is null)
        {
            return false;
        }

        if (task.State == TimedTaskState.Running)
        {
            Pause(task);
        }

        task.IsArchived = true;

        if (SelectedTask?.Id == task.Id)
        {
            SelectedTask = Tasks.FirstOrDefault();
        }

        return true;
    }

    public TimeSpan GetTotalCurrentDuration()
    {
        return _tasks
            .Where(task => !task.IsArchived)
            .Aggregate(TimeSpan.Zero, (total, task) => total + task.GetCurrentDuration());
    }

    public decimal GetTotalCurrentEarnedAmount()
    {
        return _tasks
            .Where(task => !task.IsArchived)
            .Sum(task => task.GetEarnedAmount());
    }

    public decimal GetTotalCurrentTaxSetAside()
    {
        return _tasks
            .Where(task => !task.IsArchived)
            .Sum(task => task.GetTaxSetAside());
    }

    private TimedTask? FindTask(Guid taskId)
    {
        return _tasks.FirstOrDefault(task => task.Id == taskId && !task.IsArchived);
    }

    private void PauseOtherExclusiveTasks(Guid exceptTaskId)
    {
        var runningExclusiveTasks = _tasks
            .Where(task =>
                task.Id != exceptTaskId &&
                task.Mode == TimedTaskMode.Exclusive &&
                task.State == TimedTaskState.Running &&
                !task.IsArchived)
            .ToList();

        foreach (var task in runningExclusiveTasks)
        {
            Pause(task);
        }
    }

    private static void StartOrResume(TimedTask task)
    {
        if (task.State == TimedTaskState.Running)
        {
            return;
        }

        var now = DateTimeOffset.Now;

        if (task.AccumulatedDuration == TimeSpan.Zero)
        {
            task.StartedAt = now;
        }

        task.LastStartedAt = now;
        task.EndedAt = null;
        task.State = TimedTaskState.Running;
    }

    private static void Pause(TimedTask task)
    {
        if (task.State != TimedTaskState.Running)
        {
            return;
        }

        var now = DateTimeOffset.Now;

        if (task.LastStartedAt is not null)
        {
            task.AccumulatedDuration += now - task.LastStartedAt.Value;
        }

        task.LastStartedAt = null;
        task.State = TimedTaskState.Paused;
    }
}