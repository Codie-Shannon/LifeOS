namespace LifeOS.Mobile.Core.Foundation;

public sealed class MobileStateMachine
{
    public MobileStartupState ResolveStartup(bool initialized, MobileSessionState session) =>
        !initialized ? MobileStartupState.Recovery :
        !session.IsSignedIn ? MobileStartupState.SignedOut :
        session.IsLocked ? MobileStartupState.Locked : MobileStartupState.Ready;

    public MobileSyncSnapshot Transition(MobileSyncSnapshot current, MobileSyncState next, string? safeError = null)
    {
        if (current.State == MobileSyncState.Stopped && next is not MobileSyncState.Stopped and not MobileSyncState.Idle)
            throw new InvalidOperationException("Emergency Stop must be cleared explicitly before sync can resume.");

        if (next == MobileSyncState.Failed && string.IsNullOrWhiteSpace(safeError))
            throw new ArgumentException("A redacted, user-safe failure summary is required.", nameof(safeError));

        return current with { State = next, SafeError = safeError };
    }
}
