using LifeOS.Modules.Timer.Models;

namespace LifeOS.Modules.Timer.Services;

public sealed class TimerService
{
    private TimedTask? _currentTask;

    public TimedTask? CurrentSession => _currentTask;

    public TimedTaskState State => _currentTask?.State ?? TimedTaskState.Stopped;

    public TimedTask Start(
        string contactName,
        string projectName,
        string workType,
        bool isBillable,
        decimal hourlyRate,
        decimal taxSetAsidePercent,
        string notes)
    {
        if (_currentTask is not null && _currentTask.State == TimedTaskState.Running)
        {
            throw new InvalidOperationException("A timed task is already running.");
        }

        var now = DateTimeOffset.Now;

        _currentTask = new TimedTask
        {
            Name = $"{contactName.Trim()} · {projectName.Trim()}",
            ContactName = contactName.Trim(),
            ProjectName = projectName.Trim(),
            WorkType = workType.Trim(),
            TaskType = TimedTaskType.Work,
            Mode = TimedTaskMode.Exclusive,
            IsBillable = isBillable,
            HourlyRate = hourlyRate,
            TaxSetAsidePercent = taxSetAsidePercent,
            Notes = notes.Trim(),
            StartedAt = now,
            LastStartedAt = now,
            State = TimedTaskState.Running
        };

        return _currentTask;
    }

    public void Pause()
    {
        if (_currentTask is null || _currentTask.State != TimedTaskState.Running)
        {
            return;
        }

        var now = DateTimeOffset.Now;

        if (_currentTask.LastStartedAt is not null)
        {
            _currentTask.AccumulatedDuration += now - _currentTask.LastStartedAt.Value;
        }

        _currentTask.State = TimedTaskState.Paused;
        _currentTask.LastStartedAt = null;
    }

    public void Resume()
    {
        if (_currentTask is null || _currentTask.State != TimedTaskState.Paused)
        {
            return;
        }

        _currentTask.LastStartedAt = DateTimeOffset.Now;
        _currentTask.State = TimedTaskState.Running;
    }

    public TimedTask? Stop()
    {
        if (_currentTask is null)
        {
            return null;
        }

        var now = DateTimeOffset.Now;

        if (_currentTask.State == TimedTaskState.Running)
        {
            Pause();
        }

        _currentTask.EndedAt = now;
        _currentTask.State = TimedTaskState.Done;

        var completedTask = _currentTask.CreateSnapshot(now);

        _currentTask = null;

        return completedTask;
    }

    public TimeSpan GetCurrentDuration()
    {
        return _currentTask?.GetCurrentDuration() ?? TimeSpan.Zero;
    }

    public decimal GetCurrentEarnedAmount()
    {
        return _currentTask?.GetEarnedAmount() ?? 0m;
    }

    public decimal GetCurrentTaxSetAside()
    {
        return _currentTask?.GetTaxSetAside() ?? 0m;
    }
}