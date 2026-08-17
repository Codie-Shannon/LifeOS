using LifeOS.Core.WorkSessions;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.WorkSessions;

public sealed class WorkSessionRepository
{
    private readonly VersionedJsonLocalStore<List<WorkSession>> _store;

    public WorkSessionRepository(
        string filePath,
        Func<List<WorkSession>>? emptyFactory = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        _store = new VersionedJsonLocalStore<List<WorkSession>>(
            filePath,
            "work-sessions",
            1,
            emptyFactory ?? (() => []),
            Normalize,
            utcNow: utcNow);
    }

    public LocalStoreLoadResult<List<WorkSession>> LoadResult() => _store.Load();

    public List<WorkSession> Load() => LoadResult().Value;

    public void Save(IEnumerable<WorkSession> sessions) =>
        _store.Save(sessions.ToList());

    public LocalStoreHealth Inspect() => _store.Inspect();

    public IReadOnlyList<LocalStoreTrashEntry> ListTrash() => _store.ListTrash();

    public LocalStoreTrashEntry MoveToTrash() => _store.MoveToTrash();

    public void RestoreTrash(string entryId) => _store.RestoreTrash(entryId);

    private static List<WorkSession> Normalize(List<WorkSession> sessions) => sessions
        .Select(WorkSessionService.Normalize)
        .OrderByDescending(session => session.Date)
        .ThenBy(session => session.ClientOrProject, StringComparer.OrdinalIgnoreCase)
        .ToList();
}

public static class WorkSessionStorage
{
    private const string FileName = "work-sessions.json";

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static List<WorkSession> Load() => Repository().Load();

    public static LocalStoreHealth Inspect() => Repository().Inspect();

    public static IReadOnlyList<LocalStoreTrashEntry> ListTrash() => Repository().ListTrash();

    public static void RestoreTrash(string entryId) => Repository().RestoreTrash(entryId);

    private static List<WorkSession> LoadFallback() =>
        LocalAppDataPath.IsPortfolioDemoMode ? CreateDefaultSessions() : [];

    public static void Save(IEnumerable<WorkSession> sessions)
    {
        Repository().Save(sessions);
    }

    public static void Reset()
    {
        if (File.Exists(FilePath)) Repository().MoveToTrash();
    }

    private static WorkSessionRepository Repository() => new(
        FilePath,
        LoadFallback);

    private static List<WorkSession> CreateDefaultSessions()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return
        [
            new WorkSession
            {
                ClientOrProject = "Workshop Proof Project",
                Date = today.AddDays(-1),
                Hours = 2.0m,
                HourlyRate = 45m,
                IsBillable = true,
                Status = WorkSessionStatus.Completed,
                Description = "Built and checked a local proof workflow slice.",
                Notes = "Fictional demo data. Shows completed billable work that still needs invoice/proof review."
            },
            new WorkSession
            {
                ClientOrProject = "Door Invoice OCR Proof",
                Date = today,
                Hours = 1.25m,
                HourlyRate = 45m,
                IsBillable = true,
                Status = WorkSessionStatus.Invoiced,
                Description = "Prepared review notes and screenshot proof for an invoice extraction demo.",
                Notes = "Fictional demo data. Expected money is not safe money until paid."
            }
        ];
    }
}
