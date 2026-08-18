using LifeOS.Core.LocalFirstSync;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.LocalFirstSync;

public sealed class LocalFirstSyncRepository
{
    private readonly VersionedJsonLocalStore<List<LocalAccountSyncProfile>> _store;

    public LocalFirstSyncRepository(string filePath, Func<List<LocalAccountSyncProfile>>? emptyFactory = null, Func<DateTimeOffset>? utcNow = null)
    {
        _store = new VersionedJsonLocalStore<List<LocalAccountSyncProfile>>(filePath, "local-account-sync", 1, emptyFactory ?? (() => []), Normalize, utcNow: utcNow);
    }

    public LocalStoreLoadResult<List<LocalAccountSyncProfile>> LoadResult() => _store.Load();
    public List<LocalAccountSyncProfile> Load() => LoadResult().Value;
    public void Save(IEnumerable<LocalAccountSyncProfile> profiles) => _store.Save(profiles.ToList());
    public LocalStoreHealth Inspect() => _store.Inspect();
    public IReadOnlyList<LocalStoreTrashEntry> ListTrash() => _store.ListTrash();
    public LocalStoreTrashEntry MoveToTrash() => _store.MoveToTrash();
    public void RestoreTrash(string entryId) => _store.RestoreTrash(entryId);

    private static List<LocalAccountSyncProfile> Normalize(List<LocalAccountSyncProfile> profiles)
    {
        if (profiles.Count > 1) throw new InvalidDataException("Only one local account profile is supported.");
        return profiles.Select(LocalFirstSyncService.Normalize).ToList();
    }
}

public static class LocalFirstSyncStorage
{
    private const string FileName = "local-account-sync.json";
    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);
    private static LocalFirstSyncRepository Repository() => new(FilePath, LocalAppDataPath.IsPortfolioDemoMode ? Demo : () => []);
    public static List<LocalAccountSyncProfile> Load() => Repository().Load();
    public static void Save(IEnumerable<LocalAccountSyncProfile> profiles) => Repository().Save(profiles);
    public static LocalStoreHealth Inspect() => Repository().Inspect();
    public static IReadOnlyList<LocalStoreTrashEntry> ListTrash() => Repository().ListTrash();
    public static void RestoreTrash(string entryId) => Repository().RestoreTrash(entryId);
    public static void Reset() { if (File.Exists(FilePath)) Repository().MoveToTrash(); }

    private static List<LocalAccountSyncProfile> Demo() =>
    [
        LocalFirstSyncService.RegisterLocalChange(
            LocalFirstSyncService.RegisterLocalChange(
                LocalFirstSyncService.Create(new LocalAccountDraft("Demo Operator", "Fictional Desktop", LocalAccountMode.ManualTransfer), DateTimeOffset.UtcNow.AddDays(-2)),
                DateTimeOffset.UtcNow.AddHours(-2)),
            DateTimeOffset.UtcNow.AddHours(-1))
    ];
}
