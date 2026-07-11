using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LifeOS.Core.Automation;

public static class AutomationEngine
{
    public static AutomationEvaluation Evaluate(AutomationRule rule, AutomationSourceSnapshot source, DateTimeOffset? now = null)
    {
        var validationErrors = AutomationPolicy.Validate(rule);
        if (validationErrors.Count > 0) throw new InvalidOperationException(string.Join(" ", validationErrors));

        var evaluatedAt = now ?? DateTimeOffset.UtcNow;
        var results = rule.Conditions.Select(condition => EvaluateCondition(condition, source, evaluatedAt)).ToList();
        var trustedInput = source.TrustState is AutomationTrustState.Reviewed or AutomationTrustState.Trusted;
        var matched = rule.IsEnabled && trustedInput && results.All(x => x.Passed);
        var executionMode = AutomationPolicy.ResolveExecutionMode(rule);
        var blockers = new List<string>();
        if (!rule.IsEnabled) blockers.Add("Rule is disabled.");
        if (!trustedInput) blockers.Add("Source state is untrusted and cannot create an actionable proposal.");
        if (AutomationPolicy.IsBlocked(rule.ProposedActionType)) blockers.Add("Action is blocked by the Group 27 safety policy.");
        if (!results.All(x => x.Passed)) blockers.Add("One or more deterministic conditions did not match.");

        var target = string.IsNullOrWhiteSpace(rule.TargetItemId)
            ? rule.TargetModule
            : $"{rule.TargetModule}:{rule.TargetItemId}";
        var duplicateKey = CreateDuplicateKey(rule, source, target);
        var explanation = matched
            ? $"All {results.Count} condition(s) passed against reviewed/trusted state. A proposal may be queued for explicit review."
            : string.Join(" ", blockers);

        return new AutomationEvaluation
        {
            RuleId = rule.RuleId,
            RuleRevision = rule.Revision,
            RuleSnapshot = JsonSerializer.Serialize(rule),
            SourceItemIds = [source.SourceItemId],
            SourceTrustStates = [source.TrustState],
            EvaluatedAt = evaluatedAt,
            Trigger = rule.TriggerType,
            ConditionResults = results,
            Matched = matched,
            ProposedActionType = rule.ProposedActionType,
            ProposedActionSummary = rule.ProposedActionSummary,
            Target = target,
            Risk = AutomationPolicy.ClassifyRisk(rule.ProposedActionType),
            ApprovalPolicy = AutomationPolicy.IsBlocked(rule.ProposedActionType)
                ? AutomationApprovalMode.ExecutionBlocked
                : AutomationApprovalMode.AlwaysRequireApproval,
            ExecutionMode = executionMode,
            Explanation = explanation,
            BlockingReasons = blockers,
            DuplicateProposalKey = duplicateKey,
            AuditReference = $"evaluation:{rule.RuleId}:{evaluatedAt:O}"
        };
    }

    public static AutomationProposal? CreateProposal(
        AutomationRule rule,
        AutomationEvaluation evaluation,
        IReadOnlyCollection<AutomationProposal> existing)
    {
        if (!evaluation.Matched && evaluation.ExecutionMode != AutomationExecutionMode.BlockedByPolicy) return null;

        var prior = existing
            .Where(x => x.DuplicateKey == evaluation.DuplicateProposalKey)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();
        var blocked = evaluation.ExecutionMode == AutomationExecutionMode.BlockedByPolicy;

        return new AutomationProposal
        {
            EvaluationId = evaluation.EvaluationId,
            RuleId = rule.RuleId,
            RuleName = rule.Name,
            SourceItemId = evaluation.SourceItemIds.Single(),
            ActionType = evaluation.ProposedActionType,
            ActionSummary = evaluation.ProposedActionSummary,
            Target = evaluation.Target,
            Risk = evaluation.Risk,
            ExecutionMode = evaluation.ExecutionMode,
            DuplicateKey = evaluation.DuplicateProposalKey,
            State = blocked ? AutomationProposalState.Blocked
                : prior is null ? AutomationProposalState.NeedsReview
                : AutomationProposalState.DuplicateSuspected,
            PriorProposalId = prior?.ProposalId,
            OperationalActionExecuted = false
        };
    }

    public static AutomationProposal Decide(AutomationProposal proposal, bool approve, string reason, DateTimeOffset? now = null)
    {
        if (proposal.State == AutomationProposalState.Blocked)
            throw new InvalidOperationException("Blocked proposals cannot be approved or executed.");
        if (proposal.State is AutomationProposalState.Approved or AutomationProposalState.Rejected)
            throw new InvalidOperationException("Proposal already has a retained decision.");

        return proposal with
        {
            State = approve ? AutomationProposalState.Approved : AutomationProposalState.Rejected,
            DecisionAt = now ?? DateTimeOffset.UtcNow,
            DecisionReason = string.IsNullOrWhiteSpace(reason) ? (approve ? "Approved as valid intent." : "Rejected by user.") : reason.Trim(),
            OperationalActionExecuted = false
        };
    }

    private static AutomationConditionResult EvaluateCondition(AutomationCondition condition, AutomationSourceSnapshot source, DateTimeOffset now)
    {
        source.Fields.TryGetValue(condition.Field, out var rawActual);
        var actual = rawActual ?? string.Empty;
        var expected = condition.ExpectedValue;
        var passed = condition.Type switch
        {
            AutomationConditionType.StateEquals or AutomationConditionType.ReviewStateEquals or
            AutomationConditionType.PaymentStateEquals or AutomationConditionType.TrustStateEquals or
            AutomationConditionType.TargetModuleEquals or AutomationConditionType.WaitingOnPartyEquals or
            AutomationConditionType.RiskLevelEquals => EqualsText(actual, expected),
            AutomationConditionType.StateDoesNotEqual => !EqualsText(actual, expected),
            AutomationConditionType.TextContains => actual.Contains(expected, StringComparison.OrdinalIgnoreCase),
            AutomationConditionType.EvidenceExists => ParseBool(actual),
            AutomationConditionType.EvidenceDoesNotExist => !ParseBool(actual),
            AutomationConditionType.DuplicateSuspicionPresent => ParseBool(actual) == ParseBool(expected),
            AutomationConditionType.AgeExceedsDays => ParseDecimal(actual) > ParseDecimal(expected),
            AutomationConditionType.AmountExceeds => ParseDecimal(actual) > ParseDecimal(expected),
            AutomationConditionType.DateBefore => ParseDate(actual) < ParseDate(expected),
            AutomationConditionType.DateAfter => ParseDate(actual) > ParseDate(expected),
            _ => false
        };

        return new AutomationConditionResult(
            condition.Type,
            condition.Field,
            expected,
            actual,
            passed,
            source.SourceItemId,
            source.TrustState,
            $"{condition.Field}: expected '{expected}', actual '{actual}' — {(passed ? "PASS" : "FAIL")}");
    }

    private static bool EqualsText(string left, string right) => string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
    private static bool ParseBool(string value) => bool.TryParse(value, out var result) && result;
    private static decimal ParseDecimal(string value) => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result) ? result : decimal.MinValue;
    private static DateTimeOffset ParseDate(string value) => DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result) ? result : DateTimeOffset.MinValue;

    private static string CreateDuplicateKey(AutomationRule rule, AutomationSourceSnapshot source, string target)
    {
        var relevantState = string.Join("|", source.Fields.OrderBy(x => x.Key).Select(x => $"{x.Key}={x.Value}"));
        var payload = $"{rule.RuleId}|{rule.Revision}|{source.SourceItemId}|{rule.ProposedActionType}|{target}|{relevantState}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    }
}
