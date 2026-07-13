using LifeOS.Companion.Core.Models;
namespace LifeOS.Companion.Core.Services;
public static class BetaPolicy
{
    public static string NotificationText(NotificationCategory category,bool privacySafe)=>privacySafe
        ? category switch { NotificationCategory.TransferFailed=>"A LifeOS transfer needs attention.",NotificationCategory.PairingRevoked=>"LifeOS pairing changed.",NotificationCategory.GlanceUpdated=>"LifeOS glance data was updated.",_=>"A LifeOS review is available." }
        : category switch { NotificationCategory.TransferFailed=>"Transfer failed. Open the outbox to retry manually.",NotificationCategory.PairingRevoked=>"Desktop pairing was revoked. Pending captures remain on this phone.",NotificationCategory.GlanceUpdated=>"Agenda and work glance data was updated.",_=>"A Desktop review item is available." };
    public static DeliveryState NormalizeAfterRestart(DeliveryState state)=>state==DeliveryState.Sending?DeliveryState.Pending:state;
    public static bool MayAutoSend(bool networkReturned,bool appOpened)=>false;
    public static bool IsSuccessfulAcknowledgement(Pairing.TransferResult result)=>result is Pairing.TransferResult.AcceptedForReview or Pairing.TransferResult.Duplicate;
}
public static class ConflictPolicy
{
    public static bool IsConflict(string phoneHash,string desktopHash)=>!string.Equals(phoneHash,desktopHash,StringComparison.Ordinal);
    public static CaptureConflict Resolve(CaptureConflict conflict,ConflictResolutionChoice choice,DateTimeOffset nowUtc)=>choice==ConflictResolutionChoice.Cancel?conflict:conflict with {Resolved=true,Resolution=choice,ResolvedAtUtc=nowUtc};
}
