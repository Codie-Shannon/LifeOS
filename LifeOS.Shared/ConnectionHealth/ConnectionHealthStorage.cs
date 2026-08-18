using LifeOS.Core.ConfigurationReadiness;
using LifeOS.Core.ConnectionHealth;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.ConnectionHealth;

public sealed class ConnectionHealthRepository
{
    private readonly VersionedJsonLocalStore<List<ConnectionHealthRecord>> _store;
    public ConnectionHealthRepository(string path, Func<List<ConnectionHealthRecord>>? empty = null, Func<DateTimeOffset>? utcNow = null) { _store = new(path, "connection-health", 1, empty ?? (() => []), Normalize, utcNow: utcNow); }
    public LocalStoreLoadResult<List<ConnectionHealthRecord>> LoadResult() => _store.Load(); public List<ConnectionHealthRecord> Load() => LoadResult().Value; public void Save(IEnumerable<ConnectionHealthRecord> values) => _store.Save(values.ToList()); public LocalStoreHealth Inspect() => _store.Inspect(); public IReadOnlyList<LocalStoreTrashEntry> ListTrash() => _store.ListTrash(); public LocalStoreTrashEntry MoveToTrash() => _store.MoveToTrash(); public void RestoreTrash(string id) => _store.RestoreTrash(id);
    private static List<ConnectionHealthRecord> Normalize(List<ConnectionHealthRecord> values) => values.Select(ConnectionHealthService.Normalize).OrderBy(value => value.State).ThenBy(value => value.Capability, StringComparer.OrdinalIgnoreCase).ToList();
}

public static class ConnectionHealthStorage
{
    public static string FilePath => LocalAppDataPath.GetFilePath("connection-health.json"); private static ConnectionHealthRepository Repo() => new(FilePath, LocalAppDataPath.IsPortfolioDemoMode ? Demo : () => []); public static List<ConnectionHealthRecord> Load() => Repo().Load(); public static void Save(IEnumerable<ConnectionHealthRecord> values) => Repo().Save(values); public static LocalStoreHealth Inspect() => Repo().Inspect(); public static IReadOnlyList<LocalStoreTrashEntry> ListTrash() => Repo().ListTrash(); public static void RestoreTrash(string id) => Repo().RestoreTrash(id); public static void Reset() { if (File.Exists(FilePath)) Repo().MoveToTrash(); }
    private static List<ConnectionHealthRecord> Demo() { DateTimeOffset now = DateTimeOffset.UtcNow; ConnectionHealthRecord ready = ConnectionHealthService.Create(new("Fictional calendar read", "Integration owner", ConfigurationEnvironment.Test, ConnectionCheckKind.ProviderRead, "LIFEOS_DEMO_CALENDAR_TOKEN", "Reference name only."), now.AddHours(-3)); ConnectionHealthRecord blocked = ConnectionHealthService.RecordObservation(ready with { Id = Guid.NewGuid(), Capability = "Fictional production handshake", Environment = ConfigurationEnvironment.Production, Kind = ConnectionCheckKind.AuthenticationBoundary }, ConnectionHealthState.Blocked, "Credentialed adapter is not configured.", now.AddHours(-1)); return [ready, blocked]; }
}
