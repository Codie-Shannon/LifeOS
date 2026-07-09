using LifeOS.Core.DesktopRelease;

namespace LifeOS.Shared.DesktopRelease;

public static class DesktopReleaseDemoData
{
    public static DesktopReleaseReadinessProfile CreateDefaultProfile()
    {
        return new DesktopReleaseReadinessProfile
        {
            Version = "v2.0",
            ReleaseLane = "Paid desktop release candidate",
            IsPaidDesktopCandidate = true,
            LocalOnlyMode = true,
            RequiresManualReview = true,
            TreatExpectedMoneyAsUnsafe = true,
            ScreenshotPrivacyRequired = true,
            DemoSafeDataRequired = true,
            ExternalIntegrationsEnabled = false,
            ReleaseNote = "v2.0 paid desktop release readiness checkpoint",
            LastReviewedAt = DateTime.Now,
            Checklist =
            [
                Complete("Command Centre is the operating home", DesktopReleaseCheckArea.CoreWorkflow, "Command Centre aggregates pressure, paid work, follow-ups, daily flow, and safety signals."),
                Complete("Group 01 screenshots/docs complete", DesktopReleaseCheckArea.Screenshots, "Evidence + Relationships have a captured visual checkpoint."),
                Complete("Group 02 screenshots/docs complete", DesktopReleaseCheckArea.Screenshots, "Daily Operating Flow has a captured visual checkpoint."),
                Complete("Group 03 screenshots/docs complete", DesktopReleaseCheckArea.Screenshots, "Paid Work / Money / Proof has a captured visual checkpoint."),
                Complete("Group 04 screenshots/docs complete", DesktopReleaseCheckArea.Screenshots, "Settings / Safety / Theme has a captured visual checkpoint."),
                Complete("Expected money is not safe", DesktopReleaseCheckArea.DataSafety, "Pipeline value and pending manual income stay separate from safe-to-spend money."),
                Complete("Manual review gates are on", DesktopReleaseCheckArea.DataSafety, "LifeOS prepares text and summaries but does not send messages automatically."),
                Complete("Screenshot privacy is documented", DesktopReleaseCheckArea.DemoSafety, "Docs and screenshots must avoid real names, emails, IDs, URLs, and secrets."),
                Complete("Sidebar navigation is reachable", DesktopReleaseCheckArea.DesktopUsability, "Navigation scroll hotfix keeps lower modules reachable on smaller windows."),
                Complete("One-script apply packs are standard", DesktopReleaseCheckArea.ReleaseOps, "Packs extract, apply, build, commit, push, and tag where appropriate."),
                Review("README reflects the current release", DesktopReleaseCheckArea.Documentation, "Final v2.0 screenshots should refresh the embedded README images."),
                Review("Final v2.0 screenshot group captured", DesktopReleaseCheckArea.Screenshots, "Group 05 screenshots are still needed before tagging v2.0."),
                Planned("Installer / signed release package", DesktopReleaseCheckArea.ReleaseOps, "Future release hardening; not required for the local-first roadmap checkpoint."),
                Planned("External integrations", DesktopReleaseCheckArea.ReleaseOps, "Integrations remain v4+ work after the offline foundation is stable.")
            ]
        };
    }

    private static DesktopReleaseChecklistItem Complete(string title, DesktopReleaseCheckArea area, string notes)
    {
        return new DesktopReleaseChecklistItem
        {
            Title = title,
            Area = area,
            Status = DesktopReleaseCheckStatus.Complete,
            Priority = 3,
            Notes = notes,
            UpdatedAt = DateTime.Now
        };
    }

    private static DesktopReleaseChecklistItem Review(string title, DesktopReleaseCheckArea area, string notes)
    {
        return new DesktopReleaseChecklistItem
        {
            Title = title,
            Area = area,
            Status = DesktopReleaseCheckStatus.ReviewNeeded,
            Priority = 1,
            Notes = notes,
            UpdatedAt = DateTime.Now
        };
    }

    private static DesktopReleaseChecklistItem Planned(string title, DesktopReleaseCheckArea area, string notes)
    {
        return new DesktopReleaseChecklistItem
        {
            Title = title,
            Area = area,
            Status = DesktopReleaseCheckStatus.PlannedNext,
            Priority = 5,
            Notes = notes,
            UpdatedAt = DateTime.Now
        };
    }
}
