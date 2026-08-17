using LifeOS.Core.Agenda;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.Agenda;

public static class AgendaStorage
{
    private const string FileName = "agenda-items.json";

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static List<AgendaItem> Load() => Store().Load().Value;

    public static LocalStoreHealth Inspect() => Store().Inspect();

    public static IReadOnlyList<LocalStoreTrashEntry> ListTrash() => Store().ListTrash();

    public static void RestoreTrash(string entryId) => Store().RestoreTrash(entryId);

    private static List<AgendaItem> LoadFallback() =>
        LocalAppDataPath.IsPortfolioDemoMode ? CreateDefaultItems() : [];

    public static void Save(IEnumerable<AgendaItem> items)
    {
        Store().Save(items.ToList());
    }

    public static void Reset()
    {
        if (File.Exists(FilePath)) Store().MoveToTrash();
    }

    private static VersionedJsonLocalStore<List<AgendaItem>> Store() => new(
        FilePath,
        "agenda",
        1,
        LoadFallback);

    private static List<AgendaItem> CreateDefaultItems()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return
        [
            new AgendaItem
            {
                Title = "Review weekly pressure",
                Type = AgendaItemType.Task,
                Status = AgendaItemStatus.Planned,
                PressureLevel = AgendaPressureLevel.Normal,
                DueDate = today,
                TimeText = "Morning",
                IsFixedCommitment = false,
                Notes = "Default starter item. Replace with real weekly pressure tasks."
            },
            new AgendaItem
            {
                Title = "Follow up on paid work",
                Type = AgendaItemType.FollowUp,
                Status = AgendaItemStatus.Planned,
                PressureLevel = AgendaPressureLevel.High,
                DueDate = today.AddDays(2),
                TimeText = "Any time",
                IsFixedCommitment = false,
                Notes = "Demo item for v0.2."
            }
        ];
    }
}
