namespace LifeOS.Core.DailyOperatingFlow;

public enum DailyOperatingFlowKind
{
    Anchor = 0,
    NextAction = 10,
    WaitingCheckpoint = 20,
    AdminSweep = 30,
    ProofCapture = 40,
    LowEnergyFallback = 50,
    RecoveryBlock = 60,
    StopPoint = 70
}
