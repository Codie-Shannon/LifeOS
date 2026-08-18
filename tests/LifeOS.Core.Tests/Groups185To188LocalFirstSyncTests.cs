using System.Text.Json;
using LifeOS.Core.Forms;
using LifeOS.Core.LocalFirstSync;
using LifeOS.Shared.LocalFirstSync;
using LifeOS.Shared.Storage;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups185To188LocalFirstSyncTests
{
    [Fact] public void Profile_requires_display_name_and_device_label()
    {
        FormValidationResult result = LocalFirstSyncService.Validate(Draft() with { DisplayName = null, DeviceLabel = null });
        Assert.False(result.IsValid);
        Assert.Contains(result.ForField("local-account-name"), issue => issue.Code == "required");
        Assert.Contains(result.ForField("local-device-label"), issue => issue.Code == "required");
    }

    [Fact] public void Profile_requires_bounded_single_line_labels()
    {
        FormValidationResult result = LocalFirstSyncService.Validate(Draft() with { DisplayName = "bad\nline", DeviceLabel = new string('x', 81) });
        Assert.Equal("single-line", Assert.Single(result.ForField("local-account-name")).Code);
        Assert.Equal("maximum-length", Assert.Single(result.ForField("local-device-label")).Code);
    }

    [Fact] public void Create_never_enables_provider_or_background_sync()
    {
        LocalAccountSyncProfile profile = LocalFirstSyncService.Create(Draft(), Now);
        Assert.Equal(LocalSyncState.LocalOnly, profile.State);
        Assert.False(profile.ProviderConfigured);
        Assert.False(profile.BackgroundSyncEnabled);
    }

    [Fact] public void Local_changes_and_manual_transfer_are_explicit()
    {
        LocalAccountSyncProfile profile = LocalFirstSyncService.Create(Draft() with { Mode = LocalAccountMode.ManualTransfer }, Now);
        profile = LocalFirstSyncService.RegisterLocalChange(profile, Now.AddMinutes(1));
        Assert.Equal(1, profile.PendingLocalChanges);
        profile = LocalFirstSyncService.RecordManualTransfer(profile, Now.AddMinutes(2));
        Assert.Equal(0, profile.PendingLocalChanges);
        Assert.Equal(Now.AddMinutes(2), profile.LastManualTransferUtc);
    }

    [Fact] public void Missing_repository_is_empty_without_creating_a_file()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "local-account-sync.json"); LocalFirstSyncRepository repository = new(path);
        LocalStoreLoadResult<List<LocalAccountSyncProfile>> result = repository.LoadResult();
        Assert.Equal(LocalStoreLoadState.Empty, result.State); Assert.Empty(result.Value); Assert.False(File.Exists(path));
    }

    [Fact] public void Repository_round_trips_one_versioned_normalized_profile()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "local-account-sync.json"); LocalFirstSyncRepository repository = new(path);
        repository.Save([LocalFirstSyncService.Create(Draft() with { DisplayName = "  Local Operator  " }, Now)]);
        LocalAccountSyncProfile loaded = Assert.Single(repository.Load()); using JsonDocument json = JsonDocument.Parse(File.ReadAllText(path));
        Assert.Equal("Local Operator", loaded.DisplayName); Assert.Equal("local-account-sync", json.RootElement.GetProperty("storeId").GetString()); Assert.Equal(1, json.RootElement.GetProperty("schemaVersion").GetInt32());
    }

    [Fact] public void Repository_rejects_enabled_provider_without_overwrite()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "local-account-sync.json"); LocalFirstSyncRepository repository = new(path);
        LocalAccountSyncProfile valid = LocalFirstSyncService.Create(Draft(), Now); repository.Save([valid]);
        Assert.Throws<InvalidDataException>(() => repository.Save([valid with { ProviderConfigured = true }])); Assert.False(Assert.Single(repository.Load()).ProviderConfigured);
    }

    [Fact] public void Trash_restore_is_recoverable_and_refuses_overwrite()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "local-account-sync.json"); LocalFirstSyncRepository repository = new(path);
        repository.Save([LocalFirstSyncService.Create(Draft(), Now)]); LocalStoreTrashEntry trash = repository.MoveToTrash();
        repository.RestoreTrash(trash.Id); Assert.Single(repository.Load()); Assert.Throws<InvalidOperationException>(() => repository.RestoreTrash(trash.Id));
    }

    private static readonly DateTimeOffset Now = new(2026, 8, 18, 1, 2, 3, TimeSpan.Zero);
    private static LocalAccountDraft Draft() => new("Local Operator", "Primary Desktop", LocalAccountMode.LocalOnly);
    private sealed class TemporaryDirectory : IDisposable { public TemporaryDirectory() { Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "lifeos-local-sync-tests-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(Path); } public string Path { get; } public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true); } }
}
