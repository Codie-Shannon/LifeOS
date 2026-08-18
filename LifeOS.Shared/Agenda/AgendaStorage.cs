using LifeOS.Core.Agenda;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.Agenda;

public sealed class AgendaRepository
{
    private readonly VersionedJsonLocalStore<List<AgendaItem>> _store;
    public AgendaRepository(string filePath, Func<List<AgendaItem>>? emptyFactory = null, Func<DateTimeOffset>? utcNow = null)
    {
        _store = new VersionedJsonLocalStore<List<AgendaItem>>(filePath, "agenda", 1, emptyFactory ?? (() => []), Normalize, utcNow: utcNow);
    }
    public LocalStoreLoadResult<List<AgendaItem>> LoadResult() => _store.Load();
    public List<AgendaItem> Load() => LoadResult().Value;
    public void Save(IEnumerable<AgendaItem> items) => _store.Save(items.ToList());
    public LocalStoreHealth Inspect() => _store.Inspect();
    public IReadOnlyList<LocalStoreTrashEntry> ListTrash() => _store.ListTrash();
    public LocalStoreTrashEntry MoveToTrash() => _store.MoveToTrash();
    public void RestoreTrash(string entryId) => _store.RestoreTrash(entryId);
    private static List<AgendaItem> Normalize(List<AgendaItem> items) => items.Select(AgendaService.Normalize)
        .OrderBy(item => item.Status is AgendaItemStatus.Completed or AgendaItemStatus.Cancelled)
        .ThenByDescending(item => item.IsFixedCommitment).ThenBy(item => item.DueDate ?? DateOnly.MaxValue)
        .ThenByDescending(item => item.PressureLevel).ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase).ToList();
}

public static class AgendaStorage
{
    private const string FileName = "agenda-items.json";

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static List<AgendaItem> Load() => Repository().Load();

    public static LocalStoreHealth Inspect() => Repository().Inspect();

    public static IReadOnlyList<LocalStoreTrashEntry> ListTrash() => Repository().ListTrash();

    public static void RestoreTrash(string entryId) => Repository().RestoreTrash(entryId);

    private static List<AgendaItem> LoadFallback() =>
        LocalAppDataPath.IsPortfolioDemoMode ? CreateDefaultItems() : [];

    public static void Save(IEnumerable<AgendaItem> items)
    {
        Repository().Save(items);
    }

    public static void Reset()
    {
        if (File.Exists(FilePath)) Repository().MoveToTrash();
    }

    private static AgendaRepository Repository() => new(FilePath, LoadFallback);

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
                NextAction = "Review the fictional pressure list.",
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
                NextAction = "Review the fictional follow-up context.",
                IsFixedCommitment = false,
                Notes = "Demo item for v0.2."
            }
        ];
    }
}
