namespace LifeOS.Core.DesktopRelease;

public static class DesktopReleaseReadinessCalculator
{
    public static DesktopReleaseReadinessSummary Calculate(DesktopReleaseReadinessProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var checklist = profile.Checklist ?? [];
        var total = checklist.Count;
        var complete = checklist.Count(item => item.Status == DesktopReleaseCheckStatus.Complete);
        var review = checklist.Count(item => item.Status == DesktopReleaseCheckStatus.ReviewNeeded);
        var planned = checklist.Count(item => item.Status == DesktopReleaseCheckStatus.PlannedNext);
        var blocked = checklist.Count(item => item.Status == DesktopReleaseCheckStatus.Blocked);

        var score = total == 0
            ? 0
            : (int)Math.Round((decimal)complete / total * 100m, MidpointRounding.AwayFromZero);

        var reasons = new List<string>();

        if (profile.LocalOnlyMode)
        {
            reasons.Add("Local-only mode is active; integrations remain future work.");
        }
        else
        {
            reasons.Add("Local-only mode is off. Do not treat this as a safe paid release without explicit integration review.");
        }

        if (profile.RequiresManualReview)
        {
            reasons.Add("Manual review remains required before any client/admin wording leaves LifeOS.");
        }

        if (profile.TreatExpectedMoneyAsUnsafe)
        {
            reasons.Add("Expected money remains protected and separate from safe-to-spend money.");
        }

        if (profile.ScreenshotPrivacyRequired && profile.DemoSafeDataRequired)
        {
            reasons.Add("Screenshot privacy and demo-safe data rules are required for public documentation.");
        }

        if (profile.ExternalIntegrationsEnabled)
        {
            reasons.Add("External integrations are enabled; this should not be treated as the offline paid desktop release lane.");
        }
        else
        {
            reasons.Add("External integrations are deliberately off for v2.0 paid desktop release readiness.");
        }

        if (blocked > 0)
        {
            reasons.Add($"{blocked} release check(s) are blocked and must be fixed before tagging the release.");
        }
        else if (review > 0)
        {
            reasons.Add($"{review} release check(s) need review before the final screenshot/docs tag.");
        }
        else
        {
            reasons.Add("No blocked release checks are present.");
        }

        var state = blocked > 0
            ? "Blocked"
            : review > 0
                ? "Review candidate"
                : "Paid desktop candidate";

        return new DesktopReleaseReadinessSummary
        {
            Version = profile.Version,
            ReleaseLane = profile.ReleaseLane,
            ReleaseStateLabel = state,
            TotalChecks = total,
            CompleteChecks = complete,
            ReviewNeededChecks = review,
            PlannedNextChecks = planned,
            BlockedChecks = blocked,
            ScorePercent = score,
            IsLocalOnly = profile.LocalOnlyMode,
            IsManualReviewProtected = profile.RequiresManualReview,
            IsExpectedMoneyProtected = profile.TreatExpectedMoneyAsUnsafe,
            IsScreenshotSafe = profile.ScreenshotPrivacyRequired && profile.DemoSafeDataRequired,
            Reasons = reasons,
            PriorityItems = checklist
                .Where(item => item.Status is DesktopReleaseCheckStatus.ReviewNeeded or DesktopReleaseCheckStatus.Blocked)
                .OrderBy(item => item.Priority)
                .ThenBy(item => item.Area)
                .ToList(),
            CompletedItems = checklist
                .Where(item => item.Status == DesktopReleaseCheckStatus.Complete)
                .OrderBy(item => item.Area)
                .ThenBy(item => item.Priority)
                .ToList(),
            PlannedNextItems = checklist
                .Where(item => item.Status == DesktopReleaseCheckStatus.PlannedNext)
                .OrderBy(item => item.Priority)
                .ThenBy(item => item.Area)
                .ToList()
        };
    }

    public static string FormatArea(DesktopReleaseCheckArea area)
    {
        return area switch
        {
            DesktopReleaseCheckArea.CoreWorkflow => "Core workflow",
            DesktopReleaseCheckArea.DataSafety => "Data safety",
            DesktopReleaseCheckArea.Documentation => "Documentation",
            DesktopReleaseCheckArea.Screenshots => "Screenshots",
            DesktopReleaseCheckArea.ReleaseOps => "Release ops",
            DesktopReleaseCheckArea.DemoSafety => "Demo safety",
            DesktopReleaseCheckArea.DesktopUsability => "Desktop usability",
            _ => area.ToString()
        };
    }

    public static string FormatStatus(DesktopReleaseCheckStatus status)
    {
        return status switch
        {
            DesktopReleaseCheckStatus.Complete => "Complete",
            DesktopReleaseCheckStatus.ReviewNeeded => "Review needed",
            DesktopReleaseCheckStatus.PlannedNext => "Planned next",
            DesktopReleaseCheckStatus.Blocked => "Blocked",
            _ => status.ToString()
        };
    }
}
