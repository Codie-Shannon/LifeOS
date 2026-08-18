using LifeOS.Core.WeeklyReview;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.WeeklyReview;

public sealed class WeeklyReviewRepository
{
    private readonly VersionedJsonLocalStore<List<WeeklyReviewRecord>> _store;

    public WeeklyReviewRepository(string filePath, Func<List<WeeklyReviewRecord>>? emptyFactory = null, Func<DateTimeOffset>? utcNow = null)
    {
        _store = new VersionedJsonLocalStore<List<WeeklyReviewRecord>>(
            filePath, "weekly-review", 1, emptyFactory ?? (() => []), Normalize, utcNow: utcNow);
    }

    public LocalStoreLoadResult<List<WeeklyReviewRecord>> LoadResult() => _store.Load();
    public List<WeeklyReviewRecord> Load() => LoadResult().Value;
    public void Save(IEnumerable<WeeklyReviewRecord> records) => _store.Save(records.ToList());
    public LocalStoreHealth Inspect() => _store.Inspect();
    public IReadOnlyList<LocalStoreTrashEntry> ListTrash() => _store.ListTrash();
    public LocalStoreTrashEntry MoveToTrash() => _store.MoveToTrash();
    public void RestoreTrash(string entryId) => _store.RestoreTrash(entryId);

    private static List<WeeklyReviewRecord> Normalize(List<WeeklyReviewRecord> records) => records
        .Select(WeeklyReviewService.Normalize)
        .OrderBy(record => record.State == WeeklyReviewState.Archived)
        .ThenByDescending(record => record.WeekStart)
        .ThenBy(record => record.Id, StringComparer.Ordinal)
        .ToList();
}

public static class WeeklyReviewStorage
{
    private const string FileName = "weekly-review.json";
    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);
    public static List<WeeklyReviewRecord> Load() => Repository().Load();
    public static void Save(IEnumerable<WeeklyReviewRecord> records) => Repository().Save(records);
    public static LocalStoreHealth Inspect() => Repository().Inspect();
    public static IReadOnlyList<LocalStoreTrashEntry> ListTrash() => Repository().ListTrash();
    public static void RestoreTrash(string entryId) => Repository().RestoreTrash(entryId);
    public static void Reset() { if (File.Exists(FilePath)) Repository().MoveToTrash(); }
    private static WeeklyReviewRepository Repository() => new(FilePath, LocalAppDataPath.IsPortfolioDemoMode ? Demo : () => []);

    private static List<WeeklyReviewRecord> Demo()
    {
        DateTimeOffset now = new(2026, 8, 18, 8, 0, 0, TimeSpan.FromHours(12));
        return
        [
            WeeklyReviewService.Create(new("2026-08-17", "Closed three fictional delivery milestones.", "Moved the fictional onboarding review into next week.", "Waiting on a fictional client sample.", WeeklyReviewPressure.High, "Protect two focused delivery blocks.", "Portfolio-only review; no external action was taken."), now) with { State = WeeklyReviewState.Ready },
            WeeklyReviewService.Create(new("2026-08-10", "Reconciled the fictional weekly plan.", "Moved one low-pressure household item.", "Nothing outstanding.", WeeklyReviewPressure.Normal, "Keep the next review concise.", null), now) with { State = WeeklyReviewState.Closed }
        ];
    }
}
