using LifeOS.Companion.Core;using LifeOS.Companion.Core.Models;using LifeOS.Companion.Core.Pairing;using LifeOS.Companion.Core.Services;
namespace LifeOS.Companion.Tests;
public sealed class Group34Tests
{
 [Fact]public void Beta_identity_is_exact()=>Assert.Equal("0.1.0-beta.1",ProductIdentity.Version);
 [Fact]public void Notifications_are_private_by_default_policy()=>Assert.DoesNotContain("transfer failed",BetaPolicy.NotificationText(NotificationCategory.TransferFailed,true),StringComparison.OrdinalIgnoreCase);
 [Fact]public void Restart_normalizes_stale_sending()=>Assert.Equal(DeliveryState.Pending,BetaPolicy.NormalizeAfterRestart(DeliveryState.Sending));
 [Fact]public void Reconnect_never_auto_sends(){Assert.False(BetaPolicy.MayAutoSend(true,true));Assert.False(BetaPolicy.MayAutoSend(true,false));}
 [Fact]public void Same_capture_changed_content_is_conflict()=>Assert.True(ConflictPolicy.IsConflict("phone","desktop"));
 [Fact]public void Matching_content_is_not_conflict()=>Assert.False(ConflictPolicy.IsConflict("same","same"));
 [Fact]public void Cancel_does_not_resolve_conflict(){var c=new CaptureConflict("x","c","p","d",DateTimeOffset.UtcNow,false,null,null);Assert.False(ConflictPolicy.Resolve(c,ConflictResolutionChoice.Cancel,DateTimeOffset.UtcNow).Resolved);}
 [Theory][InlineData(ConflictResolutionChoice.KeepPhoneDraft)][InlineData(ConflictResolutionChoice.KeepDesktopVersion)][InlineData(ConflictResolutionChoice.CreateSeparateReviewItem)]public void Explicit_choice_resolves_and_records(ConflictResolutionChoice choice){var c=new CaptureConflict("x","c","p","d",DateTimeOffset.UtcNow,false,null,null);var r=ConflictPolicy.Resolve(c,choice,DateTimeOffset.UtcNow);Assert.True(r.Resolved);Assert.Equal(choice,r.Resolution);}
 [Fact]public void Only_verified_success_delivers(){Assert.True(BetaPolicy.IsSuccessfulAcknowledgement(TransferResult.AcceptedForReview));Assert.True(BetaPolicy.IsSuccessfulAcknowledgement(TransferResult.Duplicate));Assert.False(BetaPolicy.IsSuccessfulAcknowledgement(TransferResult.Conflict));}
 [Fact]public void Product_has_no_cloud_or_background_sync(){Assert.False(ProductIdentity.HasCloudAccount);Assert.False(ProductIdentity.HasBackgroundSync);}
}
