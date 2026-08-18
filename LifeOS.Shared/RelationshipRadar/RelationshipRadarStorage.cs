using LifeOS.Core.RelationshipRadar;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.RelationshipRadar;

public sealed class RelationshipRadarRepository
{
    private readonly VersionedJsonLocalStore<List<RelationshipRadarProfile>> _store;
    public RelationshipRadarRepository(string filePath, Func<List<RelationshipRadarProfile>>? emptyFactory = null, Func<DateTimeOffset>? utcNow = null) { _store = new VersionedJsonLocalStore<List<RelationshipRadarProfile>>(filePath, "relationship-radar", 1, emptyFactory ?? (() => []), Normalize, utcNow: utcNow); }
    public LocalStoreLoadResult<List<RelationshipRadarProfile>> LoadResult() => _store.Load();
    public List<RelationshipRadarProfile> Load() => LoadResult().Value;
    public void Save(IEnumerable<RelationshipRadarProfile> profiles) => _store.Save(profiles.ToList());
    public LocalStoreHealth Inspect() => _store.Inspect();
    public IReadOnlyList<LocalStoreTrashEntry> ListTrash() => _store.ListTrash();
    public LocalStoreTrashEntry MoveToTrash() => _store.MoveToTrash();
    public void RestoreTrash(string entryId) => _store.RestoreTrash(entryId);
    private static List<RelationshipRadarProfile> Normalize(List<RelationshipRadarProfile> profiles) => profiles.Select(RelationshipRadarService.Normalize).OrderBy(profile => profile.Status == RelationshipRadarStatus.Closed).ThenBy(profile => profile.DoNotChase).ThenBy(profile => profile.NextFollowUpDate ?? DateOnly.MaxValue).ThenBy(profile => profile.Name, StringComparer.OrdinalIgnoreCase).ToList();
}

public static class RelationshipRadarStorage
{
    private const string FileName = "relationship-radar.json";
    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);
    private static RelationshipRadarRepository Repository() => new(FilePath, LocalAppDataPath.IsPortfolioDemoMode ? RelationshipRadarDemoData.CreateDefaultProfiles : () => []);
    public static List<RelationshipRadarProfile> Load() => Repository().Load();
    public static void Save(IEnumerable<RelationshipRadarProfile> profiles) => Repository().Save(profiles);
    public static LocalStoreHealth Inspect() => Repository().Inspect();
    public static IReadOnlyList<LocalStoreTrashEntry> ListTrash() => Repository().ListTrash();
    public static void RestoreTrash(string entryId) => Repository().RestoreTrash(entryId);
    public static void ResetToDemoData() { if (File.Exists(FilePath)) Repository().MoveToTrash(); }
}
