using LifeOS.Core.Life;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.Life;

public sealed class LifeRoutineRepository
{
    private readonly VersionedJsonLocalStore<List<LifeRoutineRecord>> _store;
    public LifeRoutineRepository(string filePath, Func<List<LifeRoutineRecord>>? emptyFactory = null, Func<DateTimeOffset>? utcNow = null)
    {
        _store = new VersionedJsonLocalStore<List<LifeRoutineRecord>>(
            filePath, "life-routines", 1, emptyFactory ?? (() => []), Normalize, utcNow: utcNow);
    }
    public LocalStoreLoadResult<List<LifeRoutineRecord>> LoadResult() => _store.Load();
    public List<LifeRoutineRecord> Load() => LoadResult().Value;
    public void Save(IEnumerable<LifeRoutineRecord> records) => _store.Save(records.ToList());
    public LocalStoreHealth Inspect() => _store.Inspect();
    public IReadOnlyList<LocalStoreTrashEntry> ListTrash() => _store.ListTrash();
    public LocalStoreTrashEntry MoveToTrash() => _store.MoveToTrash();
    public void RestoreTrash(string entryId) => _store.RestoreTrash(entryId);
    private static List<LifeRoutineRecord> Normalize(List<LifeRoutineRecord> records) => records
        .Select(LifeRoutineService.Normalize)
        .OrderBy(record => record.State == LifeRoutineState.Archived)
        .ThenBy(record => record.State == LifeRoutineState.Done)
        .ThenBy(record => record.Date)
        .ThenByDescending(record => record.Pinned)
        .ThenBy(record => record.Title, StringComparer.OrdinalIgnoreCase)
        .ToList();
}

public static class LifeRoutineStorage
{
    private const string FileName = "life-routines.json";
    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);
    public static List<LifeRoutineRecord> Load() => Repository().Load();
    public static void Save(IEnumerable<LifeRoutineRecord> records) => Repository().Save(records);
    public static LocalStoreHealth Inspect() => Repository().Inspect();
    public static IReadOnlyList<LocalStoreTrashEntry> ListTrash() => Repository().ListTrash();
    public static void RestoreTrash(string entryId) => Repository().RestoreTrash(entryId);
    public static void Reset() { if (File.Exists(FilePath)) Repository().MoveToTrash(); }
    private static LifeRoutineRepository Repository() => new(FilePath, LocalAppDataPath.IsPortfolioDemoMode ? Demo : () => []);
    private static List<LifeRoutineRecord> Demo()
    {
        DateTimeOffset now = new(2026, 8, 18, 8, 0, 0, TimeSpan.FromHours(12));
        return
        [
            LifeRoutineService.Create(new("2026-08-18", "Fictional morning reset", "Wellbeing", LifeRoutineKind.Wellbeing, LifeRoutinePressure.Normal, "Review the fictional morning checklist.", "07:30-08:00", "Portfolio-only routine.", true), now) with { State = LifeRoutineState.Active },
            LifeRoutineService.Create(new("2026-08-19", "Fictional vehicle renewal", "Personal admin", LifeRoutineKind.PersonalAdmin, LifeRoutinePressure.High, "Review the fictional renewal date.", null, "No provider write or payment.", false), now),
            LifeRoutineService.Create(new("2026-08-20", "Fictional home maintenance", "Household", LifeRoutineKind.Maintenance, LifeRoutinePressure.Low, "Confirm the fictional maintenance window.", "18:00", null, false), now) with { State = LifeRoutineState.Waiting }
        ];
    }
}
