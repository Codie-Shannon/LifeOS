using LifeOS.Modules.Timer.Models;

namespace LifeOS.Modules.Timer.Services;

public sealed class TimerService
{
    private TimerSession? _currentSession;
    private DateTimeOffset? _lastStartedAt;

    public TimerSession? CurrentSession => _currentSession;

    public TimerState State => _currentSession?.State ?? TimerState.Stopped;

    public TimerSession Start(
        string clientName,
        string projectName,
        string workType,
        bool isBillable,
        decimal hourlyRate,
        decimal taxSetAsidePercent,
        string notes)
    {
        if (_currentSession is not null && _currentSession.State == TimerState.Running)
        {
            throw new InvalidOperationException("A timer session is already running.");
        }

        _lastStartedAt = DateTimeOffset.Now;

        _currentSession = new TimerSession
        {
            ClientName = clientName.Trim(),
            ProjectName = projectName.Trim(),
            WorkType = workType.Trim(),
            IsBillable = isBillable,
            HourlyRate = hourlyRate,
            TaxSetAsidePercent = taxSetAsidePercent,
            Notes = notes.Trim(),
            State = TimerState.Running
        };

        return _currentSession;
    }

    public void Pause()
    {
        if (_currentSession is null || _currentSession.State != TimerState.Running)
        {
            return;
        }

        var now = DateTimeOffset.Now;
        var startedAt = _lastStartedAt ?? _currentSession.StartedAt;

        _currentSession.AccumulatedDuration += now - startedAt;
        _currentSession.State = TimerState.Paused;
        _lastStartedAt = null;
    }

    public void Resume()
    {
        if (_currentSession is null || _currentSession.State != TimerState.Paused)
        {
            return;
        }

        _lastStartedAt = DateTimeOffset.Now;
        _currentSession.State = TimerState.Running;
    }

    public TimerSession? Stop()
    {
        if (_currentSession is null)
        {
            return null;
        }

        if (_currentSession.State == TimerState.Running)
        {
            Pause();
        }

        _currentSession.EndedAt = DateTimeOffset.Now;
        _currentSession.State = TimerState.Stopped;

        var completedSession = _currentSession;
        _currentSession = null;
        _lastStartedAt = null;

        return completedSession;
    }

    public TimeSpan GetCurrentDuration()
    {
        if (_currentSession is null)
        {
            return TimeSpan.Zero;
        }

        if (_currentSession.State == TimerState.Running)
        {
            var now = DateTimeOffset.Now;
            var startedAt = _lastStartedAt ?? _currentSession.StartedAt;
            return _currentSession.AccumulatedDuration + (now - startedAt);
        }

        return _currentSession.AccumulatedDuration;
    }

    public decimal GetCurrentEarnedAmount()
    {
        if (_currentSession is null || !_currentSession.IsBillable)
        {
            return 0m;
        }

        var hours = (decimal)GetCurrentDuration().TotalHours;
        return Math.Round(hours * _currentSession.HourlyRate, 2);
    }

    public decimal GetCurrentTaxSetAside()
    {
        if (_currentSession is null)
        {
            return 0m;
        }

        return Math.Round(GetCurrentEarnedAmount() * (_currentSession.TaxSetAsidePercent / 100m), 2);
    }
}