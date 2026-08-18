using LifeOS.Core.FocusTimers;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.FocusTimers;

public sealed class FocusTimerRepository
{
    private readonly VersionedJsonLocalStore<List<FocusTimerRecord>> _store;
    public FocusTimerRepository(string filePath, Func<List<FocusTimerRecord>>? emptyFactory = null, Func<DateTimeOffset>? utcNow = null)
    {
        _store = new VersionedJsonLocalStore<List<FocusTimerRecord>>(filePath, "focus-timers", 1, emptyFactory ?? (() => []), Normalize, utcNow: utcNow);
    }
    public LocalStoreLoadResult<List<FocusTimerRecord>> LoadResult() => _store.Load();
    public List<FocusTimerRecord> Load() => LoadResult().Value;
    public void Save(IEnumerable<FocusTimerRecord> records) => _store.Save(records.ToList());
    public LocalStoreHealth Inspect() => _store.Inspect();
    public IReadOnlyList<LocalStoreTrashEntry> ListTrash() => _store.ListTrash();
    public LocalStoreTrashEntry MoveToTrash() => _store.MoveToTrash();
    public void RestoreTrash(string entryId) => _store.RestoreTrash(entryId);
    private static List<FocusTimerRecord> Normalize(List<FocusTimerRecord> records) => records.Select(FocusTimerService.Normalize)
        .OrderBy(record => record.State == FocusTimerState.Archived).ThenBy(record => record.State is FocusTimerState.Completed or FocusTimerState.Cancelled)
        .ThenByDescending(record => record.State == FocusTimerState.Running).ThenByDescending(record => record.UpdatedUtc).ToList();
}

public static class FocusTimerStorage
{
    private const string FileName = "focus-timers.json";
    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);
    public static List<FocusTimerRecord> Load() => Repository().Load();
    public static void Save(IEnumerable<FocusTimerRecord> records) => Repository().Save(records);
    public static LocalStoreHealth Inspect() => Repository().Inspect();
    public static IReadOnlyList<LocalStoreTrashEntry> ListTrash() => Repository().ListTrash();
    public static void RestoreTrash(string entryId) => Repository().RestoreTrash(entryId);
    public static void Reset() { if (File.Exists(FilePath)) Repository().MoveToTrash(); }
    private static FocusTimerRepository Repository() => new(FilePath, LocalAppDataPath.IsPortfolioDemoMode ? Demo : () => []);
    private static List<FocusTimerRecord> Demo()
    {
        DateTimeOffset now = new(2026, 8, 18, 8, 0, 0, TimeSpan.FromHours(12));
        FocusTimerRecord paused = FocusTimerService.Create(new("Fictional deep-work block", "Product", FocusTimerKind.Work, "50", "Review the fictional build checkpoint.", "Portfolio-only timer."), now);
        paused = FocusTimerService.Transition(paused, FocusTimerState.Running, now.AddMinutes(1));
        paused = FocusTimerService.Transition(paused, FocusTimerState.Paused, now.AddMinutes(26));
        FocusTimerRecord planned = FocusTimerService.Create(new("Fictional admin reset", "Personal admin", FocusTimerKind.PersonalAdmin, "20", "Review the fictional admin list.", null), now);
        return [paused, planned];
    }
}
