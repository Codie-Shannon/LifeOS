namespace LifeOS.Core.CommandCentrePressure;

public sealed class CommandCentrePressureSummary
{
    public required int TotalSignals { get; init; }

    public required int PressureScore { get; init; }

    public required string PressureLabel { get; init; }

    public required int CriticalSignals { get; init; }

    public required int HighSignals { get; init; }

    public required int ActNowSignals { get; init; }

    public required int ReviewSignals { get; init; }

    public required int WaitingSignals { get; init; }

    public required int ProtectedSignals { get; init; }

    public required int SuppressedSignals { get; init; }

    public required int UntrustedSignals { get; init; }

    public required decimal MoneyUnderPressure { get; init; }

    public required string NextSafestAction { get; init; }

    public required IReadOnlyList<string> Reasons { get; init; }

    public required IReadOnlyList<PressureSignal> TopSignals { get; init; }

    public required IReadOnlyList<PressureSignal> ActNow { get; init; }

    public required IReadOnlyList<PressureSignal> Review { get; init; }

    public required IReadOnlyList<PressureSignal> Waiting { get; init; }

    public required IReadOnlyList<PressureSignal> Protected { get; init; }
}
