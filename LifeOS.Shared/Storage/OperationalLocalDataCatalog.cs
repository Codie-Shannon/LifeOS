using LifeOS.Shared.Agenda;
using LifeOS.Shared.FollowUps;
using LifeOS.Shared.WorkPipeline;
using LifeOS.Shared.WorkSessions;

namespace LifeOS.Shared.Storage;

public sealed record OperationalLocalStoreStatus(
    string StoreId,
    string DisplayName,
    string Category,
    LocalStoreHealth Health,
    IReadOnlyList<LocalStoreTrashEntry> TrashEntries);

public static class OperationalLocalDataCatalog
{
    public static IReadOnlyList<OperationalLocalStoreStatus> Inspect() =>
    [
        Status("agenda", "Agenda", "Work", AgendaStorage.Inspect(), AgendaStorage.ListTrash()),
        Status("follow-ups", "Follow-ups", "Work", FollowUpStorage.Inspect(), FollowUpStorage.ListTrash()),
        Status("work-pipeline", "Work pipeline", "Projects", WorkPipelineStorage.Inspect(), WorkPipelineStorage.ListTrash()),
        Status("work-sessions", "Work sessions", "Time", WorkSessionStorage.Inspect(), WorkSessionStorage.ListTrash())
    ];

    public static void RestoreTrash(string storeId, string entryId)
    {
        switch (storeId)
        {
            case "agenda":
                AgendaStorage.RestoreTrash(entryId);
                break;
            case "follow-ups":
                FollowUpStorage.RestoreTrash(entryId);
                break;
            case "work-pipeline":
                WorkPipelineStorage.RestoreTrash(entryId);
                break;
            case "work-sessions":
                WorkSessionStorage.RestoreTrash(entryId);
                break;
            default:
                throw new ArgumentException("The local store is not registered.", nameof(storeId));
        }
    }

    private static OperationalLocalStoreStatus Status(
        string storeId,
        string displayName,
        string category,
        LocalStoreHealth health,
        IReadOnlyList<LocalStoreTrashEntry> trashEntries) => new(
        storeId,
        displayName,
        category,
        health,
        trashEntries);
}
