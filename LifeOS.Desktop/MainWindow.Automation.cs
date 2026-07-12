using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.Automation;
using LifeOS.Shared.Automation;

namespace LifeOS.Desktop;

public partial class MainWindow
{
    private AutomationStoreSnapshot _automationStore = AutomationStorage.Load();
    private string? _selectedAutomationRuleId;

    private void ShowAutomationCentrePage()
    {
        _automationStore = AutomationStorage.Load();
        var selected = _automationStore.Rules.FirstOrDefault(x => x.RuleId == _selectedAutomationRuleId) ?? _automationStore.Rules.FirstOrDefault();
        _selectedAutomationRuleId = selected?.RuleId;
        SetHeader("Automation Centre", "v6.0.0-alpha.3 • controlled orchestration and recovery • no unattended execution");
        var root = new StackPanel();
        root.Children.Add(CreateHeroPanel("Controlled orchestration and recovery",
            "Due work is queued for explicit review. Every executable step requires guarded confirmation. No unattended execution is enabled."));
        root.Children.Add(CreateAutomationExecutionGate());
        root.Children.Add(CreateOrchestrationOverview());

        var metrics = new WrapPanel { Margin = new Thickness(0, 18, 0, 0) };
        metrics.Children.Add(CreateDashboardCard("Rules", _automationStore.Rules.Count.ToString(), "Disabled by default"));
        metrics.Children.Add(CreateDashboardCard("Awaiting execution", _automationStore.Proposals.Count(x => x.State == AutomationProposalState.ApprovedNotExecuted).ToString(), "Separate confirmation"));
        metrics.Children.Add(CreateDashboardCard("Executed", _automationStore.Executions.Count(x => x.Succeeded && x.UndoneAt is null).ToString(), "Internal and reversible"));
        metrics.Children.Add(CreateDashboardCard("Blocked / stale", _automationStore.Proposals.Count(x => x.State is AutomationProposalState.Blocked or AutomationProposalState.Stale or AutomationProposalState.DuplicateSuspected).ToString(), "Fail closed"));
        root.Children.Add(metrics);
        root.Children.Add(CreateAutomationRuleList(selected));
        if (selected is not null) root.Children.Add(CreateAutomationRuleDetails(selected));
        root.Children.Add(CreateAutomationProposalQueue());
        root.Children.Add(CreateAutomationInternalStatePanel());
        root.Children.Add(CreateAutomationAuditPanel());
        MainContentControl.Content = root;
    }

    private Border CreateAutomationExecutionGate()
    {
        var panel = CreatePanel(); panel.Margin = new Thickness(0, 16, 0, 0);
        var stack = new StackPanel();
        var paused = _automationStore.Settings.ExecutionPaused;
        stack.Children.Add(AutomationHeading("Global execution gate", paused
            ? "PAUSED — dry-run and approval remain available, but final execution is blocked."
            : "GUARDED EXECUTION RESUMED — every action still requires approval, preview, eligibility revalidation and final confirmation."));
        var button = CreateActionButton(paused ? "Resume guarded execution" : "Pause all execution",
            paused ? Color.FromRgb(21, 94, 117) : Color.FromRgb(127, 29, 29), Colors.White);
        button.Click += (_, _) => ToggleAutomationExecutionPause();
        stack.Children.Add(button); panel.Child = stack; return panel;
    }

    private Border CreateAutomationRuleList(AutomationRule? selected)
    {
        var panel = CreatePanel(); panel.Margin = new Thickness(0, 16, 0, 0);
        var stack = new StackPanel();
        stack.Children.Add(AutomationHeading("Rules", "Rules remain local, deterministic and disabled by default. Evaluation is always manual."));
        foreach (var rule in _automationStore.Rules.Where(x => x.ArchivedAt is null))
        {
            var row = new DockPanel { Margin = new Thickness(0, 8, 0, 0) };
            var select = CreateActionButton(rule.RuleId == selected?.RuleId ? $"Selected • {rule.Name}" : rule.Name,
                Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
            select.Click += (_, _) => { _selectedAutomationRuleId = rule.RuleId; ShowAutomationCentrePage(); };
            DockPanel.SetDock(select, Dock.Left); row.Children.Add(select);
            row.Children.Add(new TextBlock { Text = $"{(rule.IsEnabled ? "Enabled" : "Disabled")} • {rule.TriggerType} • {AutomationPolicy.ClassifyRisk(rule.ProposedActionType)} • {AutomationPolicy.ResolveExecutionMode(rule)}",
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)), Margin = new Thickness(12, 8, 0, 0), TextWrapping = TextWrapping.Wrap });
            stack.Children.Add(row);
        }
        var reset = CreateActionButton("Reset fictional Group 28 demo", Color.FromRgb(51, 65, 85), Color.FromRgb(226, 232, 240));
        reset.Margin = new Thickness(0, 14, 0, 0);
        reset.Click += (_, _) => { _automationStore = AutomationStorage.ResetToDemoData(); _selectedAutomationRuleId = null; ShowAutomationCentrePage(); };
        stack.Children.Add(reset); panel.Child = stack; return panel;
    }

    private Border CreateAutomationRuleDetails(AutomationRule rule)
    {
        var panel = CreatePanel(); panel.Margin = new Thickness(0, 16, 0, 0);
        var stack = new StackPanel();
        stack.Children.Add(AutomationHeading(rule.Name, rule.Description));
        stack.Children.Add(AutomationLine("Trigger", $"{rule.TriggerType} • {rule.TriggerConfiguration}"));
        foreach (var condition in rule.Conditions) stack.Children.Add(AutomationLine("Condition", $"{condition.Field} • {condition.Type} • expected {condition.ExpectedValue}"));
        stack.Children.Add(AutomationLine("Proposed action", rule.ProposedActionSummary));
        stack.Children.Add(AutomationLine("Target", $"{rule.TargetModule}:{rule.TargetItemId}"));
        stack.Children.Add(AutomationLine("Risk", AutomationPolicy.ClassifyRisk(rule.ProposedActionType).ToString()));
        stack.Children.Add(AutomationLine("Execution eligibility", AutomationPolicy.IsExecutable(rule.ProposedActionType) ? "Typed reversible internal handler" : AutomationPolicy.IsBlocked(rule.ProposedActionType) ? "Blocked by policy" : "Proposal-only in Group 28"));
        stack.Children.Add(AutomationLine("Permissions", string.Join(" • ", rule.RequestedCapabilities.Select(x => $"{x}: {(AutomationPolicy.CapabilityAllowed(x) ? "Allowed / controlled" : "Blocked")}"))));
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 14, 0, 0) };
        var toggle = CreateActionButton(rule.IsEnabled ? "Disable rule" : "Enable manual rule", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        toggle.Click += (_, _) => ToggleAutomationRule(rule.RuleId); buttons.Children.Add(toggle);
        var evaluate = CreateActionButton("Evaluate dry run", Color.FromRgb(15, 118, 110), Color.FromRgb(240, 253, 250));
        evaluate.Margin = new Thickness(10, 0, 0, 0); evaluate.Click += (_, _) => EvaluateAutomationRule(rule.RuleId); buttons.Children.Add(evaluate);
        stack.Children.Add(buttons);
        var latest = _automationStore.Evaluations.Where(x => x.RuleId == rule.RuleId).OrderByDescending(x => x.EvaluatedAt).FirstOrDefault();
        if (latest is not null)
        {
            stack.Children.Add(AutomationHeading("Latest dry-run explanation", latest.Explanation));
            foreach (var result in latest.ConditionResults) stack.Children.Add(AutomationLine(result.Passed ? "PASS" : "FAIL", result.Explanation + $" • Trust: {result.SourceTrustState}"));
        }
        panel.Child = stack; return panel;
    }

    private Border CreateAutomationProposalQueue()
    {
        var panel = CreatePanel(); panel.Margin = new Thickness(0, 16, 0, 0);
        var stack = new StackPanel();
        stack.Children.Add(AutomationHeading("Guarded approval queue", "Approval never executes. Eligible proposals require a separate final preview and explicit Execute confirmation."));
        if (_automationStore.Proposals.Count == 0) stack.Children.Add(AutomationLine("Queue", "No proposals yet."));
        foreach (var proposal in _automationStore.Proposals.OrderByDescending(x => x.CreatedAt).Take(14))
        {
            stack.Children.Add(AutomationLine(proposal.State.ToString(), $"{proposal.RuleName} • {proposal.ActionSummary} • {proposal.Risk} • target {proposal.Target} • executed: {proposal.OperationalActionExecuted}"));
            if (proposal.PriorProposalId is not null) stack.Children.Add(AutomationLine("Duplicate", $"Linked to prior proposal {proposal.PriorProposalId[..Math.Min(8, proposal.PriorProposalId.Length)]}. Execution blocked."));
            AddProposalControls(stack, proposal);
            var preview = _automationStore.Previews.LastOrDefault(x => x.ProposalId == proposal.ProposalId);
            if (proposal.State == AutomationProposalState.ExecutionPreviewReady && preview is not null) AddExecutionPreview(stack, proposal, preview);
            var execution = _automationStore.Executions.LastOrDefault(x => x.ProposalId == proposal.ProposalId);
            if (execution is not null)
            {
                stack.Children.Add(AutomationLine(execution.UndoneAt is null ? "Execution result" : "Undo result",
                    $"success: {execution.Succeeded} • before retained • after retained • reversible: {execution.Reversible} • undo available: {execution.UndoAvailable}"));
            }
        }
        panel.Child = stack; return panel;
    }

    private void AddProposalControls(StackPanel stack, AutomationProposal proposal)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 10) };
        if (proposal.State == AutomationProposalState.NeedsReview)
        {
            var approve = CreateActionButton("Approve intent", Color.FromRgb(22, 101, 52), Colors.White); approve.Click += (_, _) => DecideAutomationProposal(proposal.ProposalId, true); row.Children.Add(approve);
            var reject = CreateActionButton("Reject", Color.FromRgb(127, 29, 29), Colors.White); reject.Margin = new Thickness(8, 0, 0, 0); reject.Click += (_, _) => DecideAutomationProposal(proposal.ProposalId, false); row.Children.Add(reject);
        }
        else if (proposal.State == AutomationProposalState.ApprovedNotExecuted)
        {
            var preview = CreateActionButton("Open final execution preview", Color.FromRgb(21, 94, 117), Colors.White); preview.Click += (_, _) => OpenAutomationExecutionPreview(proposal.ProposalId); row.Children.Add(preview);
            var stale = CreateActionButton("Change fictional source", Color.FromRgb(92, 62, 24), Colors.White); stale.Margin = new Thickness(8, 0, 0, 0); stale.Click += (_, _) => ChangeFictionalSource(proposal.ProposalId); row.Children.Add(stale);
        }
        else if (proposal.State == AutomationProposalState.Executed)
        {
            var execution = _automationStore.Executions.LastOrDefault(x => x.ProposalId == proposal.ProposalId);
            if (execution?.UndoAvailable == true)
            {
                var undo = CreateActionButton("Confirm Undo", Color.FromRgb(92, 62, 24), Colors.White); undo.Click += (_, _) => UndoAutomationExecution(execution.ExecutionId); row.Children.Add(undo);
            }
        }
        if (row.Children.Count > 0) stack.Children.Add(row);
    }

    private void AddExecutionPreview(StackPanel stack, AutomationProposal proposal, AutomationExecutionPreview preview)
    {
        stack.Children.Add(AutomationHeading("Final execution preview", "Review the exact before/after state. Nothing executes until Confirm guarded execution is selected."));
        stack.Children.Add(AutomationLine("Exact action", preview.ExactAction));
        stack.Children.Add(AutomationLine("Exact target", preview.ExactTarget));
        stack.Children.Add(AutomationLine("Before", preview.BeforeSnapshot));
        stack.Children.Add(AutomationLine("Proposed after", preview.ProposedAfterSnapshot));
        stack.Children.Add(AutomationLine("Risk / reversibility", $"{preview.Risk} • reversible: {preview.Reversible}"));
        foreach (var check in preview.PolicyChecks) stack.Children.Add(AutomationLine("Policy", check));
        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 12) };
        var execute = CreateActionButton("Confirm guarded execution", Color.FromRgb(22, 101, 52), Colors.White); execute.Click += (_, _) => ConfirmAutomationExecution(proposal.ProposalId); row.Children.Add(execute);
        var cancel = CreateActionButton("Cancel preview", Color.FromRgb(51, 65, 85), Colors.White); cancel.Margin = new Thickness(8, 0, 0, 0); cancel.Click += (_, _) => CancelAutomationExecutionPreview(proposal.ProposalId); row.Children.Add(cancel);
        stack.Children.Add(row);
    }

    private Border CreateAutomationInternalStatePanel()
    {
        var panel = CreatePanel(); panel.Margin = new Thickness(0, 16, 0, 0);
        var stack = new StackPanel(); stack.Children.Add(AutomationHeading("Fictional internal target state", "This is local proof data only. No external or trusted production record is changed."));
        foreach (var item in _automationStore.InternalItems)
            stack.Children.Add(AutomationLine(item.Title, $"{item.Module}:{item.ItemId} • version {item.Version} • next action {item.NextAction} • review notes {item.ReviewNotes.Count}"));
        panel.Child = stack; return panel;
    }

    private Border CreateAutomationAuditPanel()
    {
        var panel = CreatePanel(); panel.Margin = new Thickness(0, 16, 0, 0);
        var stack = new StackPanel(); stack.Children.Add(AutomationHeading("Automation audit", "Approval, preview, eligibility, execution, stale blocking, Undo and global gate changes are retained."));
        foreach (var entry in _automationStore.Audit.OrderByDescending(x => x.OccurredAt).Take(20)) stack.Children.Add(AutomationLine(entry.EventType, $"{entry.OccurredAt.LocalDateTime:g} • {entry.Summary}"));
        if (_automationStore.Audit.Count == 0) stack.Children.Add(AutomationLine("Audit", "No automation events recorded yet."));
        panel.Child = stack; return panel;
    }

    private void ToggleAutomationExecutionPause()
    {
        var paused = !_automationStore.Settings.ExecutionPaused;
        _automationStore = _automationStore with { Settings = new() { ExecutionPaused = paused, UpdatedAt = DateTimeOffset.UtcNow } };
        AddAutomationAudit(paused ? "execution-paused" : "execution-resumed", "global-execution-gate", paused ? "All guarded execution paused; dry-run remains available." : "Guarded execution resumed by explicit user action.");
        SaveAutomation(); ShowAutomationCentrePage();
    }

    private void ToggleAutomationRule(string ruleId)
    {
        var rule = _automationStore.Rules.Single(x => x.RuleId == ruleId);
        var updated = rule with { IsEnabled = !rule.IsEnabled, UpdatedAt = DateTimeOffset.UtcNow, Revision = rule.Revision + 1,
            ExecutionMode = !rule.IsEnabled ? (AutomationPolicy.IsExecutable(rule.ProposedActionType) ? AutomationExecutionMode.GuardedInternal : AutomationExecutionMode.ApprovalRequired) : AutomationExecutionMode.Disabled };
        ReplaceRule(updated); AddAutomationAudit(updated.IsEnabled ? "rule-enabled" : "rule-disabled", ruleId, $"{updated.Name} is now {(updated.IsEnabled ? "enabled for manual evaluation" : "disabled")}.");
        SaveAutomation(); ShowAutomationCentrePage();
    }

    private void EvaluateAutomationRule(string ruleId)
    {
        var rule = _automationStore.Rules.Single(x => x.RuleId == ruleId);
        var evaluation = AutomationEngine.Evaluate(rule, AutomationDemoData.SourceFor(rule, _automationStore));
        _automationStore.Evaluations.Add(evaluation); AddAutomationAudit("dry-run-started", ruleId, evaluation.Explanation);
        var proposal = AutomationEngine.CreateProposal(rule, evaluation, _automationStore.Proposals);
        if (proposal is not null)
        {
            _automationStore.Proposals.Add(proposal);
            AddAutomationAudit(proposal.State == AutomationProposalState.Blocked ? "proposal-blocked" : proposal.State == AutomationProposalState.DuplicateSuspected ? "proposal-duplicate-suspected" : "proposal-generated", proposal.ProposalId, proposal.ActionSummary);
        }
        ReplaceRule(rule with { LastEvaluatedAt = evaluation.EvaluatedAt, LastMatchedAt = evaluation.Matched ? evaluation.EvaluatedAt : rule.LastMatchedAt });
        SaveAutomation(); ShowAutomationCentrePage();
    }

    private void DecideAutomationProposal(string proposalId, bool approve)
    {
        var index = _automationStore.Proposals.FindIndex(x => x.ProposalId == proposalId);
        var decided = AutomationEngine.Decide(_automationStore.Proposals[index], approve,
            approve ? "Approved as valid intent; no execution performed. Final preview and confirmation remain required." : "Rejected during explicit review.");
        _automationStore.Proposals[index] = decided; AddAutomationAudit(approve ? "proposal-approved-not-executed" : "proposal-rejected", proposalId, decided.DecisionReason);
        SaveAutomation(); ShowAutomationCentrePage();
    }

    private void OpenAutomationExecutionPreview(string proposalId)
    {
        var index = _automationStore.Proposals.FindIndex(x => x.ProposalId == proposalId);
        var proposal = _automationStore.Proposals[index]; var rule = _automationStore.Rules.Single(x => x.RuleId == proposal.RuleId);
        var source = AutomationDemoData.SourceFor(rule, _automationStore);
        try
        {
            var preview = AutomationExecutionService.CreatePreview(_automationStore, proposal, rule, source);
            _automationStore.Previews.RemoveAll(x => x.ProposalId == proposalId); _automationStore.Previews.Add(preview);
            _automationStore.Proposals[index] = proposal with { State = AutomationProposalState.ExecutionPreviewReady };
            AddAutomationAudit("execution-preview-opened", proposalId, "Eligibility passed and final before/after preview opened.");
        }
        catch (InvalidOperationException ex)
        {
            _automationStore.Proposals[index] = proposal with { State = ex.Message.Contains("changed", StringComparison.OrdinalIgnoreCase) ? AutomationProposalState.Stale : proposal.State };
            AddAutomationAudit("execution-eligibility-failed", proposalId, ex.Message);
        }
        SaveAutomation(); ShowAutomationCentrePage();
    }

    private void ConfirmAutomationExecution(string proposalId)
    {
        var index = _automationStore.Proposals.FindIndex(x => x.ProposalId == proposalId);
        var proposal = _automationStore.Proposals[index]; var rule = _automationStore.Rules.Single(x => x.RuleId == proposal.RuleId);
        try
        {
            AddAutomationAudit("final-confirmation-accepted", proposalId, "User explicitly confirmed guarded internal execution.");
            var result = AutomationExecutionService.Execute(_automationStore, proposal, rule, AutomationDemoData.SourceFor(rule, _automationStore));
            _automationStore.Executions.Add(result);
            _automationStore.Proposals[index] = proposal with { State = AutomationProposalState.Executed, OperationalActionExecuted = true, ExecutionId = result.ExecutionId };
            AddAutomationAudit("execution-succeeded", result.ExecutionId, $"Internal reversible action completed for {proposal.Target}; before/after retained.");
        }
        catch (InvalidOperationException ex)
        {
            _automationStore.Proposals[index] = proposal with { State = ex.Message.Contains("changed", StringComparison.OrdinalIgnoreCase) ? AutomationProposalState.Stale : AutomationProposalState.ExecutionFailed };
            AddAutomationAudit("execution-failed", proposalId, ex.Message);
        }
        SaveAutomation(); ShowAutomationCentrePage();
    }

    private void CancelAutomationExecutionPreview(string proposalId)
    {
        var index = _automationStore.Proposals.FindIndex(x => x.ProposalId == proposalId);
        _automationStore.Proposals[index] = _automationStore.Proposals[index] with { State = AutomationProposalState.ApprovedNotExecuted };
        _automationStore.Previews.RemoveAll(x => x.ProposalId == proposalId);
        AddAutomationAudit("execution-preview-cancelled", proposalId, "Preview cancelled; proposal remains approved and unexecuted.");
        SaveAutomation(); ShowAutomationCentrePage();
    }

    private void ChangeFictionalSource(string proposalId)
    {
        var proposal = _automationStore.Proposals.Single(x => x.ProposalId == proposalId);
        var itemIndex = _automationStore.InternalItems.FindIndex(x => AutomationExecutionPolicy.TargetMatches(x, proposal.Target));
        if (itemIndex >= 0)
        {
            var item = _automationStore.InternalItems[itemIndex];
            _automationStore.InternalItems[itemIndex] = item with { NextAction = "Contact fictional owner", Version = item.Version + 1, UpdatedAt = DateTimeOffset.UtcNow };
            AddAutomationAudit("source-state-changed", proposalId, "Fictional source changed after approval to prove stale-state blocking.");
        }
        SaveAutomation(); ShowAutomationCentrePage();
    }

    private void UndoAutomationExecution(string executionId)
    {
        var executionIndex = _automationStore.Executions.FindIndex(x => x.ExecutionId == executionId);
        var execution = _automationStore.Executions[executionIndex];
        try
        {
            AddAutomationAudit("undo-requested", executionId, "User explicitly requested Undo.");
            var undone = AutomationExecutionService.Undo(_automationStore, execution);
            _automationStore.Executions[executionIndex] = undone;
            var proposalIndex = _automationStore.Proposals.FindIndex(x => x.ProposalId == execution.ProposalId);
            _automationStore.Proposals[proposalIndex] = _automationStore.Proposals[proposalIndex] with { State = AutomationProposalState.Undone };
            AddAutomationAudit("undo-succeeded", executionId, "Exact prior fictional internal state restored; execution history retained.");
        }
        catch (InvalidOperationException ex) { AddAutomationAudit("undo-failed", executionId, ex.Message); }
        SaveAutomation(); ShowAutomationCentrePage();
    }

    private void ReplaceRule(AutomationRule rule) { var index = _automationStore.Rules.FindIndex(x => x.RuleId == rule.RuleId); _automationStore.Rules[index] = rule; }
    private void AddAutomationAudit(string eventType, string subjectId, string summary) => _automationStore.Audit.Add(new(Guid.NewGuid().ToString("N"), DateTimeOffset.UtcNow, eventType, subjectId, summary, $"automation:{subjectId}"));
    private void SaveAutomation() => AutomationStorage.Save(_automationStore);
    private static StackPanel AutomationHeading(string title, string body) { var stack = new StackPanel { Margin = new Thickness(0, 4, 0, 8) }; stack.Children.Add(new TextBlock { Text = title, Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)), FontSize = 20, FontWeight = FontWeights.Bold, TextWrapping = TextWrapping.Wrap }); stack.Children.Add(new TextBlock { Text = body, Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)), FontSize = 13, Margin = new Thickness(0, 5, 0, 0), TextWrapping = TextWrapping.Wrap }); return stack; }
    private static TextBlock AutomationLine(string label, string value) => new() { Text = $"{label}: {value}", Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)), FontSize = 13, Margin = new Thickness(0, 5, 0, 0), TextWrapping = TextWrapping.Wrap };

    private Border CreateOrchestrationOverview()
    {
        var panel = CreatePanel(); panel.Margin = new Thickness(0, 16, 0, 0);
        var stack = new StackPanel();
        stack.Children.Add(AutomationHeading("Due Work queue", "A schedule decides when work should be reviewed. It never starts or executes a plan."));
        foreach (var plan in _automationStore.OrchestrationPlans)
        {
            var occurrence = _automationStore.OrchestrationOccurrences.LastOrDefault(x => x.PlanId == plan.PlanId);
            var run = occurrence is null ? null : _automationStore.OrchestrationRuns.LastOrDefault(x => x.OccurrenceId == occurrence.OccurrenceId);
            stack.Children.Add(AutomationLine("Plan", $"{plan.Name} • {(plan.IsEnabled ? "Enabled" : "Disabled by default")} • revision {plan.CurrentRevision}"));
            stack.Children.Add(AutomationLine("Schedule", $"{plan.ScheduleDefinition.Type} • next review {(OrchestrationService.CalculateDue(plan, DateTimeOffset.Now)?.ToString("yyyy-MM-dd") ?? "manual only")}"));
            stack.Children.Add(AutomationLine("Due occurrence", occurrence is null ? "Not created" : $"{occurrence.DueState} • {occurrence.DueAt:yyyy-MM-dd}"));
            stack.Children.Add(AutomationLine("Run", run is null ? "Not started — explicit Start required" : $"{run.Status} • current step {run.CurrentStepId ?? "complete"}"));
            stack.Children.Add(AutomationLine("Safety", "One typed reversible internal step at a time • pause between steps • no automatic retry or continuation"));
            var buttons = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
            var enable = CreateActionButton(plan.IsEnabled ? "Disable plan" : "Enable fictional plan", Color.FromRgb(30, 64, 175), Colors.White);
            enable.Click += (_, _) => ToggleOrchestrationPlan(plan.PlanId); buttons.Children.Add(enable);
            var due = CreateActionButton("Calculate due review", Color.FromRgb(51, 65, 85), Color.FromRgb(226, 232, 240));
            due.Click += (_, _) => CalculateOrchestrationDue(plan.PlanId); buttons.Children.Add(due);
            if (occurrence is not null && run is null)
            {
                var start = CreateActionButton("Start orchestration", Color.FromRgb(21, 94, 117), Colors.White);
                start.Click += (_, _) => StartOrchestration(plan.PlanId, occurrence.OccurrenceId); buttons.Children.Add(start);
            }
            if (run is not null && run.Status is OrchestrationRunStatus.Paused or OrchestrationRunStatus.RecoveryRequired)
            {
                var preview = CreateActionButton("Preview current step", Color.FromRgb(51, 65, 85), Color.FromRgb(226, 232, 240));
                preview.Click += (_, _) => PreviewOrchestrationStep(run.RunId); buttons.Children.Add(preview);
                var stepRun = run.CurrentStepId is null ? null : _automationStore.OrchestrationStepRuns.LastOrDefault(x => x.RunId == run.RunId && x.StepId == run.CurrentStepId);
                if (stepRun?.Status == OrchestrationStepStatus.PreviewReady)
                {
                    var confirm = CreateActionButton("Confirm this step only", Color.FromRgb(21, 128, 61), Colors.White);
                    confirm.Click += (_, _) => ConfirmOrchestrationStep(run.RunId); buttons.Children.Add(confirm);
                }
                if (run.Status == OrchestrationRunStatus.RecoveryRequired)
                {
                    var retry = CreateActionButton("Explicit retry", Color.FromRgb(180, 83, 9), Colors.White);
                    retry.Click += (_, _) => RetryOrchestrationStep(run.RunId); buttons.Children.Add(retry);
                    var cancel = CreateActionButton("Cancel remaining", Color.FromRgb(127, 29, 29), Colors.White);
                    cancel.Click += (_, _) => CancelOrchestration(run.RunId); buttons.Children.Add(cancel);
                }
            }
            stack.Children.Add(buttons);
            if (run is not null)
            {
                foreach (var step in _automationStore.OrchestrationSteps.Where(x => x.PlanId == plan.PlanId).OrderBy(x => x.Sequence))
                {
                    var sr = _automationStore.OrchestrationStepRuns.LastOrDefault(x => x.RunId == run.RunId && x.StepId == step.StepId);
                    stack.Children.Add(AutomationLine($"Step {step.Sequence}", $"{step.Name} • {sr?.Status.ToString() ?? "Pending"} • {step.RiskLevel} • reversible {step.IsReversible}"));
                    if (sr?.Status == OrchestrationStepStatus.PreviewReady)
                    {
                        stack.Children.Add(AutomationLine("Before", sr.BeforeSnapshot));
                        stack.Children.Add(AutomationLine("Proposed after", sr.ProposedAfterSnapshot));
                    }
                }
            }
        }
        stack.Children.Add(AutomationLine("Blocked proof", "High-risk overdue-invoice email • ExternalCommunication • blocked from execution"));
        panel.Child = stack; return panel;
    }

    private void ToggleOrchestrationPlan(string planId)
    {
        var i = _automationStore.OrchestrationPlans.FindIndex(x => x.PlanId == planId);
        var plan = _automationStore.OrchestrationPlans[i];
        _automationStore.OrchestrationPlans[i] = plan with { IsEnabled = !plan.IsEnabled, CurrentRevision = plan.CurrentRevision + 1, UpdatedAt = DateTimeOffset.UtcNow };
        AddAutomationAudit(plan.IsEnabled ? "plan-disabled" : "plan-enabled", planId, "User explicitly changed fictional orchestration plan state."); SaveAutomation(); ShowAutomationCentrePage();
    }

    private void CalculateOrchestrationDue(string planId)
    {
        var plan = _automationStore.OrchestrationPlans.Single(x => x.PlanId == planId);
        var occurrence = OrchestrationService.EnsureOccurrence(_automationStore, plan, DateTimeOffset.Now);
        AddAutomationAudit("schedule-calculated", planId, occurrence is null ? "Plan disabled or manual-only; no occurrence created." : $"Occurrence {occurrence.OccurrenceKey} exposed for review; no execution occurred."); SaveAutomation(); ShowAutomationCentrePage();
    }

    private void StartOrchestration(string planId, string occurrenceId)
    {
        try { var run = OrchestrationService.Start(_automationStore, _automationStore.OrchestrationPlans.Single(x => x.PlanId == planId), _automationStore.OrchestrationOccurrences.Single(x => x.OccurrenceId == occurrenceId)); AddAutomationAudit("run-started", run.RunId, "Explicit Start created a persisted run paused before its first step."); }
        catch (InvalidOperationException ex) { AddAutomationAudit("run-start-blocked", planId, ex.Message); }
        SaveAutomation(); ShowAutomationCentrePage();
    }

    private void PreviewOrchestrationStep(string runId)
    {
        try { var preview = OrchestrationService.PreviewCurrentStep(_automationStore, runId); AddAutomationAudit("step-preview-opened", preview.StepRunId, "Exact before/after preview opened. No mutation occurred."); }
        catch (InvalidOperationException ex) { AddAutomationAudit("step-preview-blocked", runId, ex.Message); }
        SaveAutomation(); ShowAutomationCentrePage();
    }

    private void ConfirmOrchestrationStep(string runId)
    {
        try { var result = OrchestrationService.ConfirmCurrentStep(_automationStore, runId); AddAutomationAudit("step-succeeded", result.StepRunId, "One typed reversible internal step executed; checkpoint persisted; progression paused."); }
        catch (InvalidOperationException ex) { AddAutomationAudit("step-failed", runId, ex.Message); }
        SaveAutomation(); ShowAutomationCentrePage();
    }

    private void RetryOrchestrationStep(string runId)
    {
        try { OrchestrationService.RetryFailedStep(_automationStore, runId); AddAutomationAudit("step-retry-requested", runId, "Explicit retry selected; step returned to paused review state."); }
        catch (InvalidOperationException ex) { AddAutomationAudit("step-retry-blocked", runId, ex.Message); }
        SaveAutomation(); ShowAutomationCentrePage();
    }

    private void CancelOrchestration(string runId)
    {
        OrchestrationService.CancelRemaining(_automationStore, runId); AddAutomationAudit("remaining-steps-cancelled", runId, "Future work cancelled; completed checkpoints retained."); SaveAutomation(); ShowAutomationCentrePage();
    }

}
