using System.Text.Json;
using LifeOS.Core.DeviceTransfer;
using LifeOS.Core.Forms;
using LifeOS.Shared.DeviceTransfer;
using LifeOS.Shared.Storage;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups189To192DeviceTransferTests
{
    [Fact] public void Manifest_requires_devices_record_key_and_fingerprints()
    {
        FormValidationResult result = DeviceTransferService.Validate(Draft() with { SourceDevice = null, DestinationDevice = null, RecordKey = null, LocalFingerprint = null, IncomingFingerprint = null });
        Assert.False(result.IsValid); Assert.Contains(result.ForField("transfer-source"), issue => issue.Code == "required"); Assert.Contains(result.ForField("transfer-destination"), issue => issue.Code == "required"); Assert.Contains(result.ForField("transfer-record-key"), issue => issue.Code == "required");
    }

    [Fact] public void Manifest_requires_sha256_and_single_line_labels()
    {
        FormValidationResult result = DeviceTransferService.Validate(Draft() with { SourceDevice = "bad\nline", LocalFingerprint = "abc" });
        Assert.Equal("single-line", Assert.Single(result.ForField("transfer-source")).Code); Assert.Equal("sha256", Assert.Single(result.ForField("transfer-local-fingerprint")).Code);
    }

    [Fact] public void Matching_fingerprints_are_duplicate_without_resolution()
    {
        DeviceTransferReview review = DeviceTransferService.Create(Draft() with { IncomingFingerprint = A }, Now);
        Assert.Equal(DeviceTransferState.Duplicate, review.State); Assert.Equal(DeviceTransferResolution.None, review.Resolution);
    }

    [Fact] public void Conflict_requires_explicit_decision_and_never_auto_merges()
    {
        DeviceTransferReview conflict = DeviceTransferService.Create(Draft(), Now); Assert.Equal(DeviceTransferState.ConflictReview, conflict.State);
        Assert.Throws<InvalidOperationException>(() => DeviceTransferService.Resolve(conflict, DeviceTransferResolution.None, Now.AddMinutes(1)));
        DeviceTransferReview resolved = DeviceTransferService.Resolve(conflict, DeviceTransferResolution.KeepBothCandidates, Now.AddMinutes(2)); Assert.Equal(DeviceTransferState.Resolved, resolved.State); Assert.Equal(DeviceTransferResolution.KeepBothCandidates, resolved.Resolution);
    }

    [Fact] public void Missing_repository_is_empty_without_creating_a_file()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "device-transfer-review.json"); DeviceTransferRepository repository = new(path); LocalStoreLoadResult<List<DeviceTransferReview>> result = repository.LoadResult();
        Assert.Equal(LocalStoreLoadState.Empty, result.State); Assert.Empty(result.Value); Assert.False(File.Exists(path));
    }

    [Fact] public void Repository_round_trips_versioned_normalized_manifest()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "device-transfer-review.json"); DeviceTransferRepository repository = new(path); repository.Save([DeviceTransferService.Create(Draft() with { RecordKey = "  project/demo  " }, Now)]);
        DeviceTransferReview loaded = Assert.Single(repository.Load()); using JsonDocument json = JsonDocument.Parse(File.ReadAllText(path)); Assert.Equal("project/demo", loaded.RecordKey); Assert.Equal("device-transfer-review", json.RootElement.GetProperty("storeId").GetString()); Assert.Equal(1, json.RootElement.GetProperty("schemaVersion").GetInt32());
    }

    [Fact] public void Repository_rejects_inconsistent_duplicate_without_overwrite()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "device-transfer-review.json"); DeviceTransferRepository repository = new(path); DeviceTransferReview valid = DeviceTransferService.Create(Draft(), Now); repository.Save([valid]);
        Assert.Throws<InvalidDataException>(() => repository.Save([valid with { State = DeviceTransferState.Duplicate }])); Assert.Equal(DeviceTransferState.ConflictReview, Assert.Single(repository.Load()).State);
    }

    [Fact] public void Trash_restore_is_recoverable_and_refuses_overwrite()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "device-transfer-review.json"); DeviceTransferRepository repository = new(path); repository.Save([DeviceTransferService.Create(Draft(), Now)]); LocalStoreTrashEntry trash = repository.MoveToTrash(); repository.RestoreTrash(trash.Id); Assert.Single(repository.Load()); Assert.Throws<InvalidOperationException>(() => repository.RestoreTrash(trash.Id));
    }

    private const string A = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string B = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private static readonly DateTimeOffset Now = new(2026, 8, 18, 1, 2, 3, TimeSpan.Zero);
    private static DeviceTransferDraft Draft() => new("Laptop", "Desktop", "project/demo", A, B);
    private sealed class TemporaryDirectory : IDisposable { public TemporaryDirectory() { Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "lifeos-transfer-tests-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(Path); } public string Path { get; } public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true); } }
}
