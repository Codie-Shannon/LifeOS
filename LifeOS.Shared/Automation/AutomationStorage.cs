using System.Text.Json;
using LifeOS.Core.Automation;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.Automation;

public static class AutomationStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    public static string FilePath => LocalAppDataPath.GetFilePath("lifeos-automation-v6.json");
    public static string BackupPath => LocalAppDataPath.GetFilePath("lifeos-automation-v6.backup.json");
    public static string RecoveryPath => LocalAppDataPath.GetFilePath("lifeos-automation-v6.recovery.json");
    public static string LastLoadStatus { get; private set; } = "Not loaded";

    public static AutomationStoreSnapshot Load()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        if (!File.Exists(FilePath))
        {
            LastLoadStatus = "Fresh safe store created; guarded execution paused.";
            return ResetToDemoData();
        }

        try
        {
            var loaded = Deserialize(FilePath);
            var normalized = Normalize(loaded, out var changed);
            if (changed)
            {
                File.Copy(FilePath, BackupPath, true);
                Save(normalized);
                LastLoadStatus = "Older v6 store backed up and migrated to schema 31; active work normalized to safe review state.";
            }
            else LastLoadStatus = "Current schema loaded successfully.";
            return normalized;
        }
        catch (Exception primaryError)
        {
            PreserveMalformedStore(primaryError);
            try
            {
                if (File.Exists(BackupPath))
                {
                    var backup = Normalize(Deserialize(BackupPath), out _);
                    Save(backup);
                    LastLoadStatus = "Primary store failed closed; sanitized backup restored. Review is required before execution.";
                    return backup;
                }
            }
            catch { }

            var safe = AutomationDemoData.Create() with
            {
                SchemaVersion = AutomationReleaseReadinessService.CurrentSchemaVersion,
                Settings = new() { ExecutionPaused = true, UpdatedAt = DateTimeOffset.UtcNow },
                LastNormalizedAt = DateTimeOffset.UtcNow
            };
            Save(safe);
            LastLoadStatus = "Malformed state quarantined; safe paused recovery store created. No prior state was reported as successful.";
            return safe;
        }
    }

    public static void Save(AutomationStoreSnapshot snapshot)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        if (File.Exists(FilePath)) File.Copy(FilePath, BackupPath, true);
        var temp = FilePath + ".tmp";
        var safe = snapshot with
        {
            SchemaVersion = AutomationReleaseReadinessService.CurrentSchemaVersion,
            StoreRevision = Math.Max(1, snapshot.StoreRevision),
            LastNormalizedAt = DateTimeOffset.UtcNow
        };
        File.WriteAllText(temp, JsonSerializer.Serialize(safe, Options));
        File.Move(temp, FilePath, true);
    }

    public static AutomationStoreSnapshot ResetToDemoData()
    {
        var snapshot = AutomationDemoData.Create() with
        {
            SchemaVersion = AutomationReleaseReadinessService.CurrentSchemaVersion,
            Settings = new() { ExecutionPaused = true, UpdatedAt = DateTimeOffset.UtcNow },
            LastNormalizedAt = DateTimeOffset.UtcNow
        };
        Save(snapshot);
        return snapshot;
    }

    private static AutomationStoreSnapshot Deserialize(string path) =>
        JsonSerializer.Deserialize<AutomationStoreSnapshot>(File.ReadAllText(path), Options)
        ?? throw new InvalidDataException("Automation store contained no readable state.");

    private static AutomationStoreSnapshot Normalize(AutomationStoreSnapshot snapshot, out bool changed)
    {
        changed = snapshot.SchemaVersion != AutomationReleaseReadinessService.CurrentSchemaVersion;
        if (snapshot.SchemaVersion > AutomationReleaseReadinessService.CurrentSchemaVersion)
            throw new InvalidDataException($"Unknown automation schema {snapshot.SchemaVersion}; current schema is {AutomationReleaseReadinessService.CurrentSchemaVersion}.");

        var demo = AutomationDemoData.Create();
        var proposals = snapshot.Proposals.Select(x => x.State is AutomationProposalState.Approved or AutomationProposalState.Executing
            ? x with { State = AutomationProposalState.ApprovedNotExecuted, OperationalActionExecuted = false }
            : x).ToList();
        changed |= proposals.Where((x, i) => x != snapshot.Proposals[i]).Any();

        var runs = snapshot.OrchestrationRuns.Select(x => x.Status switch
        {
            OrchestrationRunStatus.InProgress or OrchestrationRunStatus.RollingBack => x with { Status = OrchestrationRunStatus.Paused, PausedAt = DateTimeOffset.UtcNow, FailureSummary = "Restart recovery: explicit review required before continuation." },
            _ => x
        }).ToList();
        changed |= runs.Where((x, i) => x != snapshot.OrchestrationRuns[i]).Any();

        var stepRuns = snapshot.OrchestrationStepRuns.Select(x => x.Status == OrchestrationStepStatus.Executing
            ? x with { Status = OrchestrationStepStatus.Failed, FailedAt = DateTimeOffset.UtcNow, Error = "Partial execution detected after restart; recovery review required." }
            : x).ToList();
        changed |= stepRuns.Where((x, i) => x != snapshot.OrchestrationStepRuns[i]).Any();

        var plans = snapshot.OrchestrationPlans.ToList();
        var steps = snapshot.OrchestrationSteps.ToList();
        foreach (var item in demo.OrchestrationPlans.Where(d => plans.All(x => x.PlanId != d.PlanId))) { plans.Add(item); changed = true; }
        foreach (var item in demo.OrchestrationSteps.Where(d => steps.All(x => x.StepId != d.StepId))) { steps.Add(item); changed = true; }

        if (snapshot.InternalItems.Count == 0) throw new InvalidDataException("Automation store has no trusted internal state and cannot be migrated safely.");

        return snapshot with
        {
            SchemaVersion = AutomationReleaseReadinessService.CurrentSchemaVersion,
            StoreRevision = Math.Max(1, snapshot.StoreRevision),
            LastNormalizedAt = DateTimeOffset.UtcNow,
            Settings = snapshot.Settings ?? new() { ExecutionPaused = true },
            EmergencyStop = snapshot.EmergencyStop ?? new(),
            Incidents = snapshot.Incidents ?? [],
            Proposals = proposals,
            OrchestrationRuns = runs,
            OrchestrationStepRuns = stepRuns,
            OrchestrationPlans = plans,
            OrchestrationSteps = steps
        };
    }

    private static void PreserveMalformedStore(Exception error)
    {
        try
        {
            var sanitized = new
            {
                capturedAt = DateTimeOffset.UtcNow,
                source = Path.GetFileName(FilePath),
                reason = error is JsonException ? "Malformed JSON automation state." : "Automation state failed schema or safety validation.",
                action = "Fail closed; preserve source and attempt known backup."
            };
            File.WriteAllText(RecoveryPath, JsonSerializer.Serialize(sanitized, Options));
        }
        catch { }
    }
}
