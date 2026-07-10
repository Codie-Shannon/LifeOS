namespace LifeOS.Core.CommandCentrePressure;

public sealed class PressureSignal
{
    public string Key { get; set; } = string.Empty;

    public string Module { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Detail { get; set; } = string.Empty;

    public string NextAction { get; set; } = string.Empty;

    public PressureSeverity Severity { get; set; } = PressureSeverity.Normal;

    public PressureLane Lane { get; set; } = PressureLane.Review;

    public decimal MoneyAmount { get; set; }

    public bool IsTrusted { get; set; } = true;

    public bool IsDueNow { get; set; }

    public bool IsBlocked { get; set; }

    public bool IsWaitingOnOthers { get; set; }

    public bool IsSuppressed { get; set; }

    public string SuppressionReason { get; set; } = string.Empty;
}
