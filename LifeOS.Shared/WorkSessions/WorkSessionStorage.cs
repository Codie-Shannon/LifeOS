using System.Text.Json;
using LifeOS.Core.WorkSessions;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.WorkSessions;

public static class WorkSessionStorage
{
    private const string FileName = "work-sessions.json";

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static List<WorkSession> Load() => Store().Load().Value;

    public static LocalStoreHealth Inspect() => Store().Inspect();

    public static IReadOnlyList<LocalStoreTrashEntry> ListTrash() => Store().ListTrash();

    public static void RestoreTrash(string entryId) => Store().RestoreTrash(entryId);

    private static List<WorkSession> LoadFallback() =>
        LocalAppDataPath.IsPortfolioDemoMode ? CreateDefaultSessions() : [];

    public static void Save(IEnumerable<WorkSession> sessions)
    {
        Store().Save(sessions.ToList());
    }

    public static void Reset()
    {
        if (File.Exists(FilePath)) Store().MoveToTrash();
    }

    private static VersionedJsonLocalStore<List<WorkSession>> Store() => new(
        FilePath,
        "work-sessions",
        1,
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
