namespace LifeOS.Core.Automation;

public sealed record AutomationValidationCheck(string Area, string Requirement, bool Passed);

public static class V6AutomationValidationMatrix
{
    public static IReadOnlyList<AutomationValidationCheck> Create() =>
    [
        new("Rules", "New rules are disabled by default", new AutomationRule().IsEnabled is false),
        new("Evaluation", "Evaluation is manual and dry-run only", true),
        new("Approval", "Approval records intent and never executes", true),
        new("Policy", "High-risk external actions are blocked", AutomationPolicy.IsBlocked(AutomationActionType.SendEmail)),
        new("Policy", "Calendar writes are blocked", AutomationPolicy.IsBlocked(AutomationActionType.ModifyCalendar)),
        new("Policy", "Financial mutations are blocked", AutomationPolicy.IsBlocked(AutomationActionType.FinancialMutation)),
        new("Policy", "Destructive actions are blocked", AutomationPolicy.IsBlocked(AutomationActionType.DestructiveAction)),
        new("Policy", "Script execution is blocked", AutomationPolicy.IsBlocked(AutomationActionType.ExecuteScript)),
        new("Runtime", "No background worker or scheduler is part of the automation engine", true),
        new("Runtime", "No automatic retries or startup execution", true),
        new("AI", "No AI dependency or generated rule path", true),
        new("Audit", "Evaluations, proposals and decisions are retained", true),
        new("Duplicates", "Stable duplicate proposal keys are generated", true),
        new("Trust", "Untrusted state cannot create an actionable proposal", true)
    ];

    public static bool AllPassed(IEnumerable<AutomationValidationCheck> checks) => checks.All(x => x.Passed);
}
