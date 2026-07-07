namespace LifeOS.Core.CommandCentre;

public sealed class CommandCentreSnapshot
{
    public DateOnly Date { get; init; } = DateOnly.FromDateTime(DateTime.Today);

    public string OverallPressureLabel { get; init; } = "Calm";

    public string TopNextAction { get; init; } = "No urgent pressure detected. Continue planned work or pick the next project task.";

    public IReadOnlyList<CommandCentreSignal> Signals { get; init; } = [];

    public IReadOnlyList<CommandCentreSignal> TodaySignals { get; init; } = [];

    public IReadOnlyList<CommandCentreSignal> MoneySignals { get; init; } = [];

    public IReadOnlyList<CommandCentreSignal> WaitingSignals { get; init; } = [];

    public IReadOnlyList<CommandCentreSignal> ProofSignals { get; init; } = [];

    public IReadOnlyList<CommandCentreSignal> HiddenSignals { get; init; } = [];

    public int CriticalCount => Signals.Count(signal => signal.Priority == CommandCentreSignalPriority.Critical && signal.IsTodayVisible);

    public int HighCount => Signals.Count(signal => signal.Priority == CommandCentreSignalPriority.High && signal.IsTodayVisible);

    public int TodayCount => TodaySignals.Count;
}
