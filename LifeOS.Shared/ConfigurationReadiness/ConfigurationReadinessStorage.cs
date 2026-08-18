using LifeOS.Core.ConfigurationReadiness;
using LifeOS.Shared.Storage;
namespace LifeOS.Shared.ConfigurationReadiness;
public sealed class ConfigurationReadinessRepository
{
    private readonly VersionedJsonLocalStore<List<ConfigurationReadinessRecord>> _store;
    public ConfigurationReadinessRepository(string path, Func<List<ConfigurationReadinessRecord>>? empty = null, Func<DateTimeOffset>? utcNow = null) { _store = new(path, "configuration-readiness", 1, empty ?? (() => []), Normalize, utcNow: utcNow); }
    public LocalStoreLoadResult<List<ConfigurationReadinessRecord>> LoadResult() => _store.Load(); public List<ConfigurationReadinessRecord> Load() => LoadResult().Value; public void Save(IEnumerable<ConfigurationReadinessRecord> values) => _store.Save(values.ToList()); public LocalStoreHealth Inspect() => _store.Inspect(); public IReadOnlyList<LocalStoreTrashEntry> ListTrash() => _store.ListTrash(); public LocalStoreTrashEntry MoveToTrash() => _store.MoveToTrash(); public void RestoreTrash(string id) => _store.RestoreTrash(id);
    private static List<ConfigurationReadinessRecord> Normalize(List<ConfigurationReadinessRecord> values) => values.Select(ConfigurationReadinessService.Normalize).OrderBy(value => value.State).ThenBy(value => value.Capability, StringComparer.OrdinalIgnoreCase).ToList();
}
public static class ConfigurationReadinessStorage
{
    public static string FilePath => LocalAppDataPath.GetFilePath("configuration-readiness.json"); private static ConfigurationReadinessRepository Repo() => new(FilePath, LocalAppDataPath.IsPortfolioDemoMode ? Demo : () => []); public static List<ConfigurationReadinessRecord> Load() => Repo().Load(); public static void Save(IEnumerable<ConfigurationReadinessRecord> values) => Repo().Save(values); public static LocalStoreHealth Inspect() => Repo().Inspect(); public static IReadOnlyList<LocalStoreTrashEntry> ListTrash() => Repo().ListTrash(); public static void RestoreTrash(string id) => Repo().RestoreTrash(id); public static void Reset() { if (File.Exists(FilePath)) Repo().MoveToTrash(); }
    private static List<ConfigurationReadinessRecord> Demo() { DateTimeOffset now = DateTimeOffset.UtcNow; return [ConfigurationReadinessService.Create(new("Fictional sync provider", ConfigurationEnvironment.Test, "Product owner", "LIFEOS_DEMO_PROVIDER_TOKEN", "Reference name only; no credential stored."), now.AddHours(-2)), ConfigurationReadinessService.Create(new("Production deployment", ConfigurationEnvironment.Production, "Release owner", null, "Owner input is still required."), now.AddHours(-1))]; }
}
