using System.Text.Json;
using LifeOS.Core.Automation;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.Automation;

public static class AutomationStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    public static string FilePath => LocalAppDataPath.GetFilePath("lifeos-automation-v6.json");
    public static string BackupPath => LocalAppDataPath.GetFilePath("lifeos-automation-v6.backup.json");

    public static AutomationStoreSnapshot Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return ResetToDemoData();
            var loaded = JsonSerializer.Deserialize<AutomationStoreSnapshot>(File.ReadAllText(FilePath), Options);
            return Normalize(loaded ?? AutomationDemoData.Create());
        }
        catch
        {
            try
            {
                if (File.Exists(BackupPath))
                    return Normalize(JsonSerializer.Deserialize<AutomationStoreSnapshot>(File.ReadAllText(BackupPath), Options) ?? AutomationDemoData.Create());
            }
            catch { }
            return AutomationDemoData.Create();
        }
    }

    public static void Save(AutomationStoreSnapshot snapshot)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        if (File.Exists(FilePath)) File.Copy(FilePath, BackupPath, true);
        var temp = FilePath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(snapshot, Options));
        File.Move(temp, FilePath, true);
    }

    public static AutomationStoreSnapshot ResetToDemoData()
    {
        var snapshot = AutomationDemoData.Create();
        Save(snapshot);
        return snapshot;
    }

    private static AutomationStoreSnapshot Normalize(AutomationStoreSnapshot snapshot)
    {
        if (snapshot.InternalItems.Count == 0)
            return AutomationDemoData.Create();

        var demo = AutomationDemoData.Create();

        var proposals = snapshot.Proposals
            .Select(x => x.State == AutomationProposalState.Approved
                ? x with { State = AutomationProposalState.ApprovedNotExecuted }
                : x)
            .ToList();

        var runs = snapshot.OrchestrationRuns
            .Select(x => x.Status is OrchestrationRunStatus.InProgress or OrchestrationRunStatus.RollingBack
                ? x with
                {
                    Status = OrchestrationRunStatus.Paused,
                    PausedAt = DateTimeOffset.UtcNow
                }
                : x)
            .ToList();

        var plans = snapshot.OrchestrationPlans.ToList();
        var steps = snapshot.OrchestrationSteps.ToList();

        foreach (var demoPlan in demo.OrchestrationPlans)
        {
            if (plans.All(x => x.PlanId != demoPlan.PlanId))
                plans.Add(demoPlan);
        }

        foreach (var demoStep in demo.OrchestrationSteps)
        {
            if (steps.All(x => x.StepId != demoStep.StepId))
                steps.Add(demoStep);
        }

        return snapshot with
        {
            Settings = snapshot.Settings ?? new(),
            Proposals = proposals,
            OrchestrationRuns = runs,
            OrchestrationPlans = plans,
            OrchestrationSteps = steps
        };
    }
}
