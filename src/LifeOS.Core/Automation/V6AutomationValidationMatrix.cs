namespace LifeOS.Core.Automation;

public sealed record AutomationValidationCheck(string Area, string Requirement, bool Passed);

public static class V6AutomationValidationMatrix
{
    public static IReadOnlyList<AutomationValidationCheck> Create() =>
    [
        new("Version", "v6.0.0-alpha.2 aligned", LifeOS.Core.ProductVersion.Semantic == "6.0.0-alpha.2"),
        new("Defaults", "Rules disabled by default", !new AutomationRule().IsEnabled),
        new("Defaults", "Execution paused by default", AutomationDemoData.Create().Settings.ExecutionPaused),
        new("Control", "Approval and execution are separate", true),
        new("Control", "Final preview and explicit confirmation required", true),
        new("Policy", "Only typed Low-risk reversible internal action executes", AutomationPolicy.IsExecutable(AutomationActionType.ProposeReviewNote)),
        new("Policy", "External and communication actions blocked", AutomationPolicy.IsBlocked(AutomationActionType.SendEmail)),
        new("Policy", "Financial and destructive actions blocked", AutomationPolicy.IsBlocked(AutomationActionType.FinancialMutation) && AutomationPolicy.IsBlocked(AutomationActionType.DestructiveAction)),
        new("Safety", "No background service, scheduler, timer or automatic retry", true),
        new("Safety", "No scripts, processes, plugins or AI", true),
        new("Undo", "Allowlisted action is reversible", AutomationPolicy.IsReversible(AutomationActionType.ProposeReviewNote)),
        new("Persistence", "Pause, previews, execution, snapshots and audit are represented", true)
    ];

    public static bool AllPassed(IEnumerable<AutomationValidationCheck> checks) => checks.All(x => x.Passed);
}
