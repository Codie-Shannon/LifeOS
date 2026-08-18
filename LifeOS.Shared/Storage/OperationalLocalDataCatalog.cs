using LifeOS.Shared.Agenda;
using LifeOS.Shared.ConfigurationReadiness;
using LifeOS.Shared.Documents;
using LifeOS.Shared.DeviceTransfer;
using LifeOS.Shared.FollowUps;
using LifeOS.Shared.FocusTimers;
using LifeOS.Shared.Grocery;
using LifeOS.Shared.Life;
using LifeOS.Shared.LocalFirstSync;
using LifeOS.Shared.Money;
using LifeOS.Shared.Projects;
using LifeOS.Shared.RelationshipRadar;
using LifeOS.Shared.WorkPipeline;
using LifeOS.Shared.WorkSessions;
using LifeOS.Shared.WeeklyReview;

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
        Status("work-sessions", "Work sessions", "Time", WorkSessionStorage.Inspect(), WorkSessionStorage.ListTrash()),
        Status("projects", "Projects", "Projects", ProjectStorage.Inspect(), ProjectStorage.ListTrash()),
        Status("money-pressure", "Money pressure", "Money", MoneyPressureStorage.Inspect(), MoneyPressureStorage.ListTrash()),
        Status("document-intake", "Document intake", "Documents", DocumentIntakeStorage.Inspect(), DocumentIntakeStorage.ListTrash()),
        Status("household-grocery", "Household grocery", "Household", HouseholdGroceryStorage.Inspect(), HouseholdGroceryStorage.ListTrash()),
        Status("life-routines", "Life routines", "Life", LifeRoutineStorage.Inspect(), LifeRoutineStorage.ListTrash()),
        Status("weekly-review", "Weekly review", "Life", WeeklyReviewStorage.Inspect(), WeeklyReviewStorage.ListTrash()),
        Status("focus-timers", "Focus timers", "Life", FocusTimerStorage.Inspect(), FocusTimerStorage.ListTrash()),
        Status("relationship-radar", "Relationship radar", "Career", RelationshipRadarStorage.Inspect(), RelationshipRadarStorage.ListTrash()),
        Status("local-account-sync", "Local account and sync", "Settings", LocalFirstSyncStorage.Inspect(), LocalFirstSyncStorage.ListTrash()),
        Status("device-transfer-review", "Device transfer review", "Settings", DeviceTransferStorage.Inspect(), DeviceTransferStorage.ListTrash()),
        Status("configuration-readiness", "Configuration readiness", "Settings", ConfigurationReadinessStorage.Inspect(), ConfigurationReadinessStorage.ListTrash())
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
            case "projects":
                ProjectStorage.RestoreTrash(entryId);
                break;
            case "money-pressure":
                MoneyPressureStorage.RestoreTrash(entryId);
                break;
            case "document-intake":
                DocumentIntakeStorage.RestoreTrash(entryId);
                break;
            case "household-grocery":
                HouseholdGroceryStorage.RestoreTrash(entryId);
                break;
            case "life-routines":
                LifeRoutineStorage.RestoreTrash(entryId);
                break;
            case "weekly-review":
                WeeklyReviewStorage.RestoreTrash(entryId);
                break;
            case "focus-timers":
                FocusTimerStorage.RestoreTrash(entryId);
                break;
            case "relationship-radar":
                RelationshipRadarStorage.RestoreTrash(entryId);
                break;
            case "local-account-sync":
                LocalFirstSyncStorage.RestoreTrash(entryId);
                break;
            case "device-transfer-review":
                DeviceTransferStorage.RestoreTrash(entryId);
                break;
            case "configuration-readiness": ConfigurationReadinessStorage.RestoreTrash(entryId); break;
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
