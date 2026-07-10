namespace LifeOS.Core.CommandCentrePressure;

public static class CommandCentrePressureCalculator
{
    public static CommandCentrePressureSummary Calculate(
        IEnumerable<PressureSignal> signals,
        CommandCentrePressurePolicy policy)
    {
        var list = signals.ToList();

        foreach (var signal in list)
        {
            ApplySafetyRules(signal, policy);
        }

        var visible = list
            .Where(signal => !signal.IsSuppressed)
            .OrderByDescending(signal => signal.Severity)
            .ThenByDescending(signal => signal.IsDueNow)
            .ThenByDescending(signal => signal.MoneyAmount)
            .ThenBy(signal => signal.Title)
            .ToList();

        var score = visible.Sum(signal => SeverityWeight(signal.Severity, policy));
        var critical = visible.Count(signal => signal.Severity == PressureSeverity.Critical);
        var high = visible.Count(signal => signal.Severity == PressureSeverity.High);
        var actNow = visible.Where(signal => signal.Lane == PressureLane.ActNow).ToList();
        var review = visible.Where(signal => signal.Lane == PressureLane.Review).ToList();
        var waiting = visible.Where(signal => signal.Lane == PressureLane.Waiting).ToList();
        var protectedItems = list.Where(signal => signal.Lane == PressureLane.Protected || signal.IsSuppressed).ToList();
        var moneyUnderPressure = visible.Sum(signal => signal.MoneyAmount);

        var label = critical > 0 || score >= policy.CriticalScore
            ? "Critical"
            : high > 0 || score >= policy.HighScore
                ? "High"
                : score >= policy.NormalScore
                    ? "Normal"
                    : "Low";

        var nextSafestAction = actNow.FirstOrDefault()?.NextAction
            ?? review.FirstOrDefault()?.NextAction
            ?? waiting.FirstOrDefault()?.NextAction
            ?? "No immediate pressure action. Protect the current plan and review again later.";

        var reasons = new List<string>
        {
            $"{visible.Count} visible pressure signal(s) remain after safety rules.",
            $"{critical} critical and {high} high signal(s) are visible.",
            $"{actNow.Count} signal(s) can be acted on now.",
            $"{review.Count} signal(s) require review before action.",
            $"{waiting.Count} signal(s) are waiting and should not be chased early.",
            $"{protectedItems.Count} signal(s) are protected, parked, or suppressed.",
            $"{list.Count(signal => !signal.IsTrusted)} signal(s) are untrusted or review-only.",
            $"${moneyUnderPressure:0.00} is visible under pressure and is not automatically safe money.",
            $"Pressure score: {score}."
        };

        return new CommandCentrePressureSummary
        {
            TotalSignals = list.Count,
            PressureScore = score,
            PressureLabel = label,
            CriticalSignals = critical,
            HighSignals = high,
            ActNowSignals = actNow.Count,
            ReviewSignals = review.Count,
            WaitingSignals = waiting.Count,
            ProtectedSignals = protectedItems.Count,
            SuppressedSignals = list.Count(signal => signal.IsSuppressed),
            UntrustedSignals = list.Count(signal => !signal.IsTrusted),
            MoneyUnderPressure = moneyUnderPressure,
            NextSafestAction = nextSafestAction,
            Reasons = reasons,
            TopSignals = visible.Take(policy.MaximumTopSignals).ToList(),
            ActNow = actNow,
            Review = review,
            Waiting = waiting,
            Protected = protectedItems
        };
    }

    private static void ApplySafetyRules(
        PressureSignal signal,
        CommandCentrePressurePolicy policy)
    {
        if (policy.SuppressWaitingOnOthers
            && signal.IsWaitingOnOthers
            && !signal.IsDueNow
            && signal.Severity != PressureSeverity.Critical)
        {
            signal.IsSuppressed = true;
            signal.Lane = PressureLane.Protected;
            signal.SuppressionReason = "Waiting on another person and not due now.";
            return;
        }

        if (policy.RequireReviewForUntrusted && !signal.IsTrusted)
        {
            signal.Lane = PressureLane.Review;
            signal.IsSuppressed = false;
            signal.SuppressionReason = string.Empty;
            return;
        }

        if (signal.IsBlocked && !signal.IsDueNow)
        {
            signal.Lane = PressureLane.Waiting;
        }
    }

    private static int SeverityWeight(
        PressureSeverity severity,
        CommandCentrePressurePolicy policy)
    {
        return severity switch
        {
            PressureSeverity.Critical => policy.CriticalWeight,
            PressureSeverity.High => policy.HighWeight,
            PressureSeverity.Normal => policy.NormalWeight,
            _ => policy.LowWeight
        };
    }
}
