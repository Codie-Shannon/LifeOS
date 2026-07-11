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
        var selected = _automationStore.Rules.FirstOrDefault(x => x.RuleId == _selectedAutomationRuleId)
                       ?? _automationStore.Rules.FirstOrDefault();
        _selectedAutomationRuleId = selected?.RuleId;

        SetHeader("Automation Centre", "v6.0.0-alpha.1 • controlled automation foundation • manual dry-run and explicit approval only");
        var root = new StackPanel();
        root.Children.Add(CreateHeroPanel("Controlled automation foundation",
            "Dry-run and explicit approval only. No unattended actions are enabled. Approval records valid intent; it does not execute an operational change in v6.0.0-alpha.1."));

        var metrics = new WrapPanel { Margin = new Thickness(0, 18, 0, 0) };
        metrics.Children.Add(CreateDashboardCard("Rules", _automationStore.Rules.Count.ToString(), "Disabled by default"));
        metrics.Children.Add(CreateDashboardCard("Enabled", _automationStore.Rules.Count(x => x.IsEnabled).ToString(), "Dry-run only"));
        metrics.Children.Add(CreateDashboardCard("Needs review", _automationStore.Proposals.Count(x => x.State == AutomationProposalState.NeedsReview).ToString(), "Explicit decision"));
        metrics.Children.Add(CreateDashboardCard("Blocked", _automationStore.Proposals.Count(x => x.State == AutomationProposalState.Blocked).ToString(), "Policy enforced"));
        root.Children.Add(metrics);

        root.Children.Add(CreateAutomationRuleList(selected));
        if (selected is not null) root.Children.Add(CreateAutomationRuleDetails(selected));
        root.Children.Add(CreateAutomationProposalQueue());
        root.Children.Add(CreateAutomationAuditPanel());
        MainContentControl.Content = root;
    }

    private Border CreateAutomationRuleList(AutomationRule? selected)
    {
        var panel = CreatePanel(); panel.Margin = new Thickness(0, 16, 0, 0);
        var stack = new StackPanel();
        stack.Children.Add(AutomationHeading("Rules", "Rules are local, deterministic and disabled by default. Select a rule, enable it explicitly, then run a manual dry-run."));
        foreach (var rule in _automationStore.Rules.Where(x => x.ArchivedAt is null))
        {
            var row = new DockPanel { Margin = new Thickness(0, 8, 0, 0) };
            var select = CreateActionButton(rule.RuleId == selected?.RuleId ? $"Selected • {rule.Name}" : rule.Name,
                Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
            select.Click += (_, _) => { _selectedAutomationRuleId = rule.RuleId; ShowAutomationCentrePage(); };
            DockPanel.SetDock(select, Dock.Left); row.Children.Add(select);
            row.Children.Add(new TextBlock
            {
                Text = $"{(rule.IsEnabled ? "Enabled" : "Disabled")} • {rule.TriggerType} • {AutomationPolicy.ClassifyRisk(rule.ProposedActionType)} • {AutomationPolicy.ResolveExecutionMode(rule)}",
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)), Margin = new Thickness(12, 8, 0, 0), TextWrapping = TextWrapping.Wrap
            });
            stack.Children.Add(row);
        }
        var reset = CreateActionButton("Reset fictional demo rules", Color.FromRgb(51, 65, 85), Color.FromRgb(226, 232, 240));
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
        foreach (var condition in rule.Conditions)
            stack.Children.Add(AutomationLine("Condition", $"{condition.Field} • {condition.Type} • expected {condition.ExpectedValue}"));
        stack.Children.Add(AutomationLine("Proposed action", rule.ProposedActionSummary));
        stack.Children.Add(AutomationLine("Target", rule.TargetModule));
        stack.Children.Add(AutomationLine("Risk", AutomationPolicy.ClassifyRisk(rule.ProposedActionType).ToString()));
        stack.Children.Add(AutomationLine("Approval", AutomationPolicy.IsBlocked(rule.ProposedActionType) ? "Execution blocked" : "Always require approval"));
        stack.Children.Add(AutomationLine("Execution", AutomationPolicy.ResolveExecutionMode(rule).ToString()));
        stack.Children.Add(AutomationLine("Permissions", string.Join(" • ", rule.RequestedCapabilities.Select(x => $"{x}: {(AutomationPolicy.CapabilityAllowed(x) ? "Allowed / approval required" : "Blocked")}"))));

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 14, 0, 0) };
        var toggle = CreateActionButton(rule.IsEnabled ? "Disable rule" : "Enable dry-run rule", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        toggle.Click += (_, _) => ToggleAutomationRule(rule.RuleId);
        buttons.Children.Add(toggle);
        var evaluate = CreateActionButton("Evaluate dry run", Color.FromRgb(15, 118, 110), Color.FromRgb(240, 253, 250));
        evaluate.Margin = new Thickness(10, 0, 0, 0); evaluate.Click += (_, _) => EvaluateAutomationRule(rule.RuleId); buttons.Children.Add(evaluate);
        stack.Children.Add(buttons);

        var latest = _automationStore.Evaluations.Where(x => x.RuleId == rule.RuleId).OrderByDescending(x => x.EvaluatedAt).FirstOrDefault();
        if (latest is not null)
        {
            stack.Children.Add(AutomationHeading("Latest dry-run explanation", latest.Explanation));
            foreach (var result in latest.ConditionResults)
                stack.Children.Add(AutomationLine(result.Passed ? "PASS" : "FAIL", result.Explanation + $" • Trust: {result.SourceTrustState}"));
        }
        panel.Child = stack; return panel;
    }

    private Border CreateAutomationProposalQueue()
    {
        var panel = CreatePanel(); panel.Margin = new Thickness(0, 16, 0, 0);
        var stack = new StackPanel();
        stack.Children.Add(AutomationHeading("Proposed-action queue", "Approval records your decision. Execution remains disabled in v6.0.0-alpha.1."));
        if (_automationStore.Proposals.Count == 0) stack.Children.Add(AutomationLine("Queue", "No proposals yet. Enable a fictional rule and run a dry evaluation."));
        foreach (var proposal in _automationStore.Proposals.OrderByDescending(x => x.CreatedAt).Take(12))
        {
            stack.Children.Add(AutomationLine(proposal.State.ToString(), $"{proposal.RuleName} • {proposal.ActionSummary} • {proposal.Risk} • target {proposal.Target} • executed: No"));
            if (proposal.PriorProposalId is not null) stack.Children.Add(AutomationLine("Duplicate", $"Linked to prior proposal {proposal.PriorProposalId[..Math.Min(8, proposal.PriorProposalId.Length)]}."));
            if (proposal.State is AutomationProposalState.NeedsReview or AutomationProposalState.DuplicateSuspected)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 10) };
                var approve = CreateActionButton("Approve intent", Color.FromRgb(22, 101, 52), Colors.White); approve.Click += (_, _) => DecideAutomationProposal(proposal.ProposalId, true); row.Children.Add(approve);
                var reject = CreateActionButton("Reject", Color.FromRgb(127, 29, 29), Colors.White); reject.Margin = new Thickness(8, 0, 0, 0); reject.Click += (_, _) => DecideAutomationProposal(proposal.ProposalId, false); row.Children.Add(reject);
                stack.Children.Add(row);
            }
        }
        panel.Child = stack; return panel;
    }

    private Border CreateAutomationAuditPanel()
    {
        var panel = CreatePanel(); panel.Margin = new Thickness(0, 16, 0, 0);
        var stack = new StackPanel();
        stack.Children.Add(AutomationHeading("Automation audit", "Rule changes, evaluations, proposals, policy blocks and decisions remain inert and retained."));
        foreach (var entry in _automationStore.Audit.OrderByDescending(x => x.OccurredAt).Take(16))
            stack.Children.Add(AutomationLine(entry.EventType, $"{entry.OccurredAt.LocalDateTime:g} • {entry.Summary}"));
        if (_automationStore.Audit.Count == 0) stack.Children.Add(AutomationLine("Audit", "No automation events recorded yet."));
        panel.Child = stack; return panel;
    }

    private void ToggleAutomationRule(string ruleId)
    {
        var rule = _automationStore.Rules.Single(x => x.RuleId == ruleId);
        var updated = rule with { IsEnabled = !rule.IsEnabled, UpdatedAt = DateTimeOffset.UtcNow, Revision = rule.Revision + 1,
            ExecutionMode = !rule.IsEnabled ? AutomationExecutionMode.DryRunOnly : AutomationExecutionMode.Disabled };
        ReplaceRule(updated);
        AddAutomationAudit(updated.IsEnabled ? "rule-enabled" : "rule-disabled", ruleId, $"{updated.Name} is now {(updated.IsEnabled ? "enabled for manual dry-run" : "disabled") }.");
        SaveAutomation(); ShowAutomationCentrePage();
    }

    private void EvaluateAutomationRule(string ruleId)
    {
        var rule = _automationStore.Rules.Single(x => x.RuleId == ruleId);
        var evaluation = AutomationEngine.Evaluate(rule, AutomationDemoData.SourceFor(rule));
        _automationStore.Evaluations.Add(evaluation);
        AddAutomationAudit("dry-run-started", ruleId, evaluation.Explanation);
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
        var decided = AutomationEngine.Decide(_automationStore.Proposals[index], approve, approve ? "Approved as valid intent; no execution performed." : "Rejected during explicit review.");
        _automationStore.Proposals[index] = decided;
        AddAutomationAudit(approve ? "proposal-approved" : "proposal-rejected", proposalId, decided.DecisionReason);
        SaveAutomation(); ShowAutomationCentrePage();
    }

    private void ReplaceRule(AutomationRule rule)
    {
        var index = _automationStore.Rules.FindIndex(x => x.RuleId == rule.RuleId);
        _automationStore.Rules[index] = rule;
    }

    private void AddAutomationAudit(string eventType, string subjectId, string summary) => _automationStore.Audit.Add(new(
        Guid.NewGuid().ToString("N"), DateTimeOffset.UtcNow, eventType, subjectId, summary, $"automation:{subjectId}"));
    private void SaveAutomation() => AutomationStorage.Save(_automationStore);

    private static StackPanel AutomationHeading(string title, string body)
    {
        var stack = new StackPanel { Margin = new Thickness(0, 4, 0, 8) };
        stack.Children.Add(new TextBlock { Text = title, Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)), FontSize = 20, FontWeight = FontWeights.Bold, TextWrapping = TextWrapping.Wrap });
        stack.Children.Add(new TextBlock { Text = body, Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)), FontSize = 13, Margin = new Thickness(0, 5, 0, 0), TextWrapping = TextWrapping.Wrap });
        return stack;
    }

    private static TextBlock AutomationLine(string label, string value) => new()
    {
        Text = $"{label}: {value}", Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)), FontSize = 13,
        Margin = new Thickness(0, 5, 0, 0), TextWrapping = TextWrapping.Wrap
    };
}
