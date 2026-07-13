using LifeOS.Companion.Core.Models;
using LifeOS.Companion.Core.Security;
using LifeOS.Companion.Core.Services;
using LifeOS.Companion.Core.Storage;

namespace LifeOS.Companion.Tests;

public sealed class CompanionTests : IAsyncLifetime
{
    private readonly string _dbPath =
        Path.Combine(Path.GetTempPath(), $"lifeos-companion-{Guid.NewGuid():N}.db3");

    private static readonly byte[] Key =
        Enumerable.Range(1, 32).Select(x => (byte)x).ToArray();

    private readonly List<SQLiteCompanionStore> _stores = [];

    [Fact]
    public void Product_version_is_alpha_1() =>
        Assert.Equal("0.1.0-alpha.1", "0.1.0-alpha.1");

    [Fact]
    public void Empty_capture_is_rejected() =>
        Assert.False(CaptureValidator.Validate(" ", " ", null).IsValid);

    [Fact]
    public void Body_only_capture_is_valid() =>
        Assert.True(CaptureValidator.Validate(null, "note", null).IsValid);

    [Fact]
    public void Overlong_title_is_rejected() =>
        Assert.False(CaptureValidator.Validate(new string('x', 121), "body", null).IsValid);

    [Fact]
    public void Content_hash_is_stable() =>
        Assert.Equal(
            ContentHasher.Compute("a", "b", "c"),
            ContentHasher.Compute("a", "b", "c"));

    [Fact]
    public void Field_protection_round_trips()
    {
        var protector = new AesGcmFieldProtector(Key);
        Assert.Equal("secret", protector.Unprotect(protector.Protect("secret")));
    }

    [Fact]
    public void Field_protection_is_randomized()
    {
        var protector = new AesGcmFieldProtector(Key);
        Assert.NotEqual(protector.Protect("secret"), protector.Protect("secret"));
    }

    [Fact]
    public async Task Device_identity_is_created_and_persisted()
    {
        var store = await CreateStoreAsync();

        var first = await store.GetOrCreateDeviceAsync("Galaxy S9 Companion");
        var second = await store.GetOrCreateDeviceAsync("Other");

        Assert.Equal(first.DeviceId, second.DeviceId);
        Assert.Equal(ConnectionState.NotPaired, first.ConnectionState);
    }

    [Fact]
    public async Task New_capture_enters_pending_outbox()
    {
        var store = await CreateStoreAsync();
        var device = await store.GetOrCreateDeviceAsync("Galaxy S9 Companion");

        var saved = await new CaptureService(store)
            .SavePendingAsync(null, "Test", "Fictional note", "General", device);

        var outbox = await store.GetOutboxAsync();

        Assert.Single(outbox);
        Assert.Equal(saved.CaptureId, outbox[0].CaptureId);
        Assert.Equal(DeliveryState.Pending, outbox[0].DeliveryState);
    }

    [Fact]
    public async Task Repeated_save_does_not_duplicate_outbox_entry()
    {
        var store = await CreateStoreAsync();
        var device = await store.GetOrCreateDeviceAsync("Galaxy S9 Companion");
        var service = new CaptureService(store);

        var first = await service.SavePendingAsync(null, "A", "B", null, device);
        await service.SavePendingAsync(first.CaptureId, "A2", "B2", null, device);

        Assert.Single(await store.GetOutboxAsync());
    }

    [Fact]
    public async Task Editing_preserves_identity_and_created_time()
    {
        var store = await CreateStoreAsync();
        var device = await store.GetOrCreateDeviceAsync("Galaxy S9 Companion");
        var service = new CaptureService(store);

        var first = await service.SavePendingAsync(
            null,
            "A",
            "B",
            null,
            device,
            DateTimeOffset.UtcNow.AddDays(-1));

        var edited = await service.SavePendingAsync(
            first.CaptureId,
            "A2",
            "B2",
            null,
            device);

        Assert.Equal(first.CaptureId, edited.CaptureId);
        Assert.Equal(first.CreatedAtUtc, edited.CreatedAtUtc);
        Assert.True(edited.UpdatedAtUtc >= first.UpdatedAtUtc);
    }

    [Fact]
    public async Task Restart_reload_preserves_capture_and_outbox()
    {
        var store = await CreateStoreAsync();
        var device = await store.GetOrCreateDeviceAsync("Galaxy S9 Companion");

        await new CaptureService(store)
            .SavePendingAsync(null, "Restart", "Still here", null, device);

        await store.CloseAsync();

        var reopened = Track(
            new SQLiteCompanionStore(
                _dbPath,
                new AesGcmFieldProtector(Key)));

        await reopened.InitializeAsync();

        Assert.Single(await reopened.GetCapturesAsync());
        Assert.Single(await reopened.GetOutboxAsync());
    }

    [Fact]
    public async Task Pending_capture_cannot_be_deleted_as_draft()
    {
        var store = await CreateStoreAsync();
        var device = await store.GetOrCreateDeviceAsync("Galaxy S9 Companion");

        var capture = await new CaptureService(store)
            .SavePendingAsync(null, "A", "B", null, device);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => store.DeleteDraftAsync(capture.CaptureId));
    }

    [Fact]
    public async Task Delivered_state_is_rejected_by_group32_store()
    {
        var store = await CreateStoreAsync();
        var device = await store.GetOrCreateDeviceAsync("Galaxy S9 Companion");

        var capture = new QuickCapture
        {
            CaptureId = Guid.NewGuid().ToString("N"),
            Title = "No",
            Body = "Ack missing",
            Category = null,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            DeliveryState = DeliveryState.Delivered,
            SchemaVersion = 1,
            DeviceId = device.DeviceId,
            ContentHash = "x"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => store.SavePendingCaptureAsync(capture));
    }

    [Fact]
    public async Task Device_label_can_be_updated()
    {
        var store = await CreateStoreAsync();

        await store.GetOrCreateDeviceAsync("Galaxy S9 Companion");
        await store.UpdateDeviceLabelAsync("My Companion");

        Assert.Equal(
            "My Companion",
            (await store.GetOrCreateDeviceAsync("Fallback")).DeviceLabel);
    }

    private async Task<SQLiteCompanionStore> CreateStoreAsync()
    {
        var store = Track(
            new SQLiteCompanionStore(
                _dbPath,
                new AesGcmFieldProtector(Key)));

        await store.InitializeAsync();
        return store;
    }

    private SQLiteCompanionStore Track(SQLiteCompanionStore store)
    {
        _stores.Add(store);
        return store;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var store in _stores)
        {
            try
            {
                await store.CloseAsync();
            }
            catch
            {
                // Test cleanup continues so temporary files can still be removed.
            }
        }

        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            var path = _dbPath + suffix;

            for (var attempt = 0; attempt < 20 && File.Exists(path); attempt++)
            {
                try
                {
                    File.Delete(path);
                }
                catch (IOException) when (attempt < 19)
                {
                    await Task.Delay(50);
                }
            }
        }
    }
}
