using System.Text.Json;
using LifeOS.Shared.Storage;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups133To136LocalDataSpineTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 18, 21, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Missing_store_is_an_honest_empty_state_without_writing_a_file()
    {
        using TestDirectory directory = new();
        VersionedJsonLocalStore<List<TestRecord>> store = Store(directory);

        LocalStoreLoadResult<List<TestRecord>> result = store.Load();

        Assert.Equal(LocalStoreLoadState.Empty, result.State);
        Assert.Empty(result.Value);
        Assert.False(File.Exists(store.FilePath));
        Assert.Equal(LocalStoreHealthState.Missing, store.Inspect().State);
    }

    [Fact]
    public void Save_is_atomic_versioned_and_inspectable()
    {
        using TestDirectory directory = new();
        VersionedJsonLocalStore<List<TestRecord>> store = Store(directory);

        store.Save([new TestRecord("one", "First")]);

        using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(store.FilePath));
        Assert.Equal("test-records", document.RootElement.GetProperty("storeId").GetString());
        Assert.Equal(1, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Single(document.RootElement.GetProperty("payload").EnumerateArray());
        Assert.Equal(LocalStoreHealthState.Healthy, store.Inspect().State);
        Assert.Empty(Directory.GetFiles(directory.Path, "*.tmp"));
    }

    [Fact]
    public void Legacy_payload_migrates_once_and_is_preserved_as_backup()
    {
        using TestDirectory directory = new();
        VersionedJsonLocalStore<List<TestRecord>> store = Store(directory);
        File.WriteAllText(
            store.FilePath,
            JsonSerializer.Serialize(new[] { new TestRecord("legacy", "Legacy") }));

        LocalStoreLoadResult<List<TestRecord>> result = store.Load();

        Assert.Equal(LocalStoreLoadState.MigratedLegacy, result.State);
        Assert.Equal("legacy", result.Value.Single().Id);
        Assert.True(File.Exists(store.BackupPath));
        Assert.Equal(LocalStoreHealthState.Healthy, store.Inspect().State);
    }

    [Fact]
    public void Older_envelope_uses_registered_migration_and_preserves_source()
    {
        using TestDirectory directory = new();
        string path = System.IO.Path.Combine(directory.Path, "records.json");
        File.WriteAllText(path, """
            {
              "storeId": "test-records",
              "schemaVersion": 1,
              "updatedUtc": "2026-08-18T21:00:00Z",
              "payload": [{ "id": "one", "name": "Before" }]
            }
            """);
        VersionedJsonLocalStore<List<TestRecord>> store = new(
            path,
            "test-records",
            2,
            () => [],
            migrate: (schema, payload) =>
            {
                Assert.Equal(1, schema);
                List<TestRecord> records = payload.Deserialize<List<TestRecord>>(
                    new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? [];
                return records.Select(record => record with { Name = record.Name + " migrated" }).ToList();
            },
            utcNow: () => Now);

        LocalStoreLoadResult<List<TestRecord>> result = store.Load();

        Assert.Equal(LocalStoreLoadState.MigratedSchema, result.State);
        Assert.Equal("Before migrated", result.Value.Single().Name);
        Assert.True(File.Exists(store.BackupPath));
        Assert.Equal(2, store.Inspect().SchemaVersion);
    }

    [Fact]
    public void Corrupt_primary_is_preserved_and_valid_backup_is_restored()
    {
        using TestDirectory directory = new();
        VersionedJsonLocalStore<List<TestRecord>> store = Store(directory);
        store.Save([new TestRecord("one", "First")]);
        store.Save([new TestRecord("two", "Second")]);
        File.WriteAllText(store.FilePath, "{not-json");

        LocalStoreLoadResult<List<TestRecord>> result = store.Load();

        Assert.Equal(LocalStoreLoadState.RecoveredBackup, result.State);
        Assert.Equal("one", result.Value.Single().Id);
        Assert.NotNull(result.PreservedPath);
        Assert.True(File.Exists(result.PreservedPath));
        Assert.Equal(LocalStoreHealthState.Healthy, store.Inspect().State);
    }

    [Fact]
    public void Newer_schema_fails_closed_without_overwriting_the_file()
    {
        using TestDirectory directory = new();
        VersionedJsonLocalStore<List<TestRecord>> store = Store(directory);
        string newer = """
            {
              "storeId": "test-records",
              "schemaVersion": 99,
              "updatedUtc": "2026-08-18T21:00:00Z",
              "payload": [{ "id": "future", "name": "Future" }]
            }
            """;
        File.WriteAllText(store.FilePath, newer);

        LocalStoreLoadResult<List<TestRecord>> result = store.Load();

        Assert.Equal(LocalStoreLoadState.UnsupportedNewerSchema, result.State);
        Assert.Empty(result.Value);
        Assert.Equal(newer, File.ReadAllText(store.FilePath));
        Assert.Equal(LocalStoreHealthState.NewerSchema, store.Inspect().State);

        Assert.Throws<InvalidOperationException>(() =>
            store.Save([new TestRecord("older-client", "Must not overwrite")]));
        Assert.Equal(newer, File.ReadAllText(store.FilePath));
    }

    [Fact]
    public void Trash_restore_rejects_manifest_paths_outside_the_registered_trash_directory()
    {
        using TestDirectory directory = new();
        VersionedJsonLocalStore<List<TestRecord>> store = Store(directory);
        store.Save([new TestRecord("one", "First")]);
        LocalStoreTrashEntry entry = store.MoveToTrash();
        string external = System.IO.Path.Combine(directory.Path, "outside.json");
        File.WriteAllText(external, "do not move");
        string manifest = Directory.GetFiles(directory.Path, entry.Id + ".trash.json", SearchOption.AllDirectories).Single();
        LocalStoreTrashEntry tampered = entry with { TrashPath = external };
        File.WriteAllText(manifest, JsonSerializer.Serialize(tampered));

        Assert.Throws<InvalidDataException>(() => store.RestoreTrash(entry.Id));
        Assert.True(File.Exists(external));
        Assert.False(File.Exists(store.FilePath));
        Assert.Equal("do not move", File.ReadAllText(external));
    }

    [Fact]
    public void Trash_is_recoverable_and_restore_refuses_to_overwrite_current_data()
    {
        using TestDirectory directory = new();
        VersionedJsonLocalStore<List<TestRecord>> store = Store(directory);
        store.Save([new TestRecord("one", "First")]);

        LocalStoreTrashEntry entry = store.MoveToTrash();

        Assert.False(File.Exists(store.FilePath));
        Assert.Single(store.ListTrash());
        Assert.Equal(Now.AddDays(30), entry.PurgeAfterUtc);

        store.RestoreTrash(entry.Id);
        Assert.Equal("one", store.Load().Value.Single().Id);
        Assert.Empty(store.ListTrash());

        store.MoveToTrash();
        store.Save([new TestRecord("two", "Current")]);
        LocalStoreTrashEntry pending = store.ListTrash().Single();
        Assert.Throws<InvalidOperationException>(() => store.RestoreTrash(pending.Id));
    }

    private static VersionedJsonLocalStore<List<TestRecord>> Store(TestDirectory directory) => new(
        System.IO.Path.Combine(directory.Path, "records.json"),
        "test-records",
        1,
        () => [],
        utcNow: () => Now);

    private sealed record TestRecord(string Id, string Name);

    private sealed class TestDirectory : IDisposable
    {
        public TestDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"lifeos-local-store-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
