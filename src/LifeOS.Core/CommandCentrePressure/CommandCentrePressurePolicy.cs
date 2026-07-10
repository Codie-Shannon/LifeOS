namespace LifeOS.Core.CommandCentrePressure;

public sealed class CommandCentrePressurePolicy
{
    public int LowWeight { get; set; } = 1;

    public int NormalWeight { get; set; } = 3;

    public int HighWeight { get; set; } = 7;

    public int CriticalWeight { get; set; } = 12;

    public int NormalScore { get; set; } = 5;

    public int HighScore { get; set; } = 16;

    public int CriticalScore { get; set; } = 30;

    public int MaximumTopSignals { get; set; } = 8;

    public bool SuppressWaitingOnOthers { get; set; } = true;

    public bool RequireReviewForUntrusted { get; set; } = true;
}
