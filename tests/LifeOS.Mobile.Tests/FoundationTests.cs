using Xunit;
using System.Security.Cryptography;
using LifeOS.Mobile.Core.Foundation;
using LifeOS.Mobile.Core.Services;
using LifeOS.Mobile.Core.Storage;

namespace LifeOS.Mobile.Tests;

public sealed class FoundationTests
{
    [Fact] public void AllEightPermanentWorkspacesRemainReachable() => Assert.Equal(8, MobileWorkspaceCatalog.Permanent.Distinct().Count());

    [Fact] public void StartupStateHonoursSignedOutAndLockedBoundaries()
    {
        var sut = new MobileStateMachine();
        Assert.Equal(MobileStartupState.SignedOut, sut.ResolveStartup(true, new(false, false, "device")));
        Assert.Equal(MobileStartupState.Locked, sut.ResolveStartup(true, new(true, true, "device")));
        Assert.Equal(MobileStartupState.Ready, sut.ResolveStartup(true, new(true, false, "device")));
    }

    [Fact] public void EmergencyStopBlocksImplicitSyncResume()
    {
        var sut = new MobileStateMachine();
        var stopped = new MobileSyncSnapshot(MobileSyncState.Stopped, 1, null, "Offline");
        Assert.Throws<InvalidOperationException>(() => sut.Transition(stopped, MobileSyncState.Syncing));
    }

    [Fact] public void DiagnosticsRedactEmailsAndSecrets()
    {
        var value = Redaction.Safe("user@example.com token:abcdef password=hunter2");
        Assert.DoesNotContain("user@example.com", value);
        Assert.DoesNotContain("abcdef", value);
        Assert.DoesNotContain("hunter2", value);
    }

    [Fact] public async Task EncryptedStorePersistsSettingsAndIdempotentOutbox()
    {
        var dir = Path.Combine(Path.GetTempPath(), "lifeos-mobile-tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(dir, "store.bin"); var key = RandomNumberGenerator.GetBytes(32);
        try
        {
            var store = new JsonMobileLocalStore(path, key); await store.InitializeAsync();
            var item = new MobileOutboxItem("fixed-id", "test", MobileFoundationService.Sha256("payload"), DateTimeOffset.UtcNow, "Pending");
            await store.QueueAsync(item); await store.QueueAsync(item);
            Assert.Single(await store.LoadOutboxAsync());
            var bytes = await File.ReadAllBytesAsync(path);
            Assert.DoesNotContain("fixed-id", System.Text.Encoding.UTF8.GetString(bytes));
        }
        finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    [Fact] public async Task ClearDeviceDataRemovesPersistedStore()
    {
        var dir = Path.Combine(Path.GetTempPath(), "lifeos-mobile-tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(dir, "store.bin");
        var store = new JsonMobileLocalStore(path, RandomNumberGenerator.GetBytes(32));
        await store.InitializeAsync(); Assert.True(File.Exists(path));
        await store.ClearDeviceDataAsync(); Assert.False(File.Exists(path));
    }
}
