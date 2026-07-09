using LifeOS.Core.UniversalSpine;

namespace LifeOS.Shared.UniversalSpine;

public static class UniversalSpineDemoData
{
    public static UniversalSpineProfile CreateDefaultProfile()
    {
        var paidWork = Item("Package paid work release proof", UniversalSpineItemKind.Work, UniversalSpineSignalStatus.Active, 1, "Paid Work Centre", "Attach proof and keep expected money separate from safe money.", "Connects paid work, proof, invoice readiness, and money safety.");
        var proof = Item("Group 05 release screenshots", UniversalSpineItemKind.Proof, UniversalSpineSignalStatus.Complete, 2, "Evidence Vault", "Keep renamed screenshots in docs and README.", "Screenshots prove the v2.0 release checkpoint.");
        var relationship = Item("Client-safe follow-up wording", UniversalSpineItemKind.Relationship, UniversalSpineSignalStatus.ReviewNeeded, 1, "Relationship Radar", "Review wording before any manual client/admin update.", "Relationship state should influence follow-up timing without exposing real inboxes.");
        var dailyFlow = Item("Next safe build block", UniversalSpineItemKind.DailyFlow, UniversalSpineSignalStatus.Active, 2, "Daily Operating Flow", "Build one pack, run it, screenshot the group, then docs/tag.", "Keeps work moving without opening too many loops.");
        var money = Item("Expected money is not safe", UniversalSpineItemKind.Money, UniversalSpineSignalStatus.Waiting, 1, "Money Pressure", "Keep pending and expected money out of safe-to-spend.", "The money spine blocks false confidence.");
        var settings = Item("Screenshot privacy guardrail", UniversalSpineItemKind.Settings, UniversalSpineSignalStatus.Active, 2, "Settings / Safety", "Keep demo data fictional and remove private identifiers.", "Protects docs and portfolio screenshots.");
        var release = Item("v2.1 universal spine code checkpoint", UniversalSpineItemKind.Release, UniversalSpineSignalStatus.ReviewNeeded, 1, "Desktop Release", "Capture Group 06 screenshots before tagging v2.1.", "The release centre should connect to the new spine instead of being isolated.");
        var knowledge = Item("Docs index stays current", UniversalSpineItemKind.Knowledge, UniversalSpineSignalStatus.Active, 3, "Docs", "Update README, current status, version history, and screenshot groups every time.", "Prevents stale documents from contradicting the build.");

        return new UniversalSpineProfile
        {
            Version = "v2.1",
            SpineMode = "Local universal spine",
            UniversalSearchPlanned = true,
            CrossModuleLinksEnabled = true,
            ManualReviewRequired = true,
            ExternalSyncEnabled = false,
            Notes = "Universal spine foundation for local cross-module context and links.",
            Items =
            [
                paidWork,
                proof,
                relationship,
                dailyFlow,
                money,
                settings,
                release,
                knowledge
            ],
            Links =
            [
                Link(paidWork, proof, UniversalSpineLinkType.NeedsProof, "Paid work needs proof", "Work should not become invoice-ready without proof context."),
                Link(paidWork, money, UniversalSpineLinkType.ProducesMoney, "Work produces expected money", "Expected money stays unsafe until paid."),
                Link(relationship, paidWork, UniversalSpineLinkType.NeedsFollowUp, "Follow-up can move paid work", "Manual wording may unblock paid-work admin."),
                Link(dailyFlow, paidWork, UniversalSpineLinkType.Supports, "Daily block supports paid work", "Daily flow decides what moves today."),
                Link(settings, proof, UniversalSpineLinkType.Documents, "Privacy protects screenshots", "Screenshot privacy guardrails apply to proof/docs."),
                Link(release, proof, UniversalSpineLinkType.NeedsProof, "Release needs screenshots", "Release tag waits for screenshot docs."),
                Link(knowledge, release, UniversalSpineLinkType.Documents, "Docs document release", "README and version history explain the current build.")
            ]
        };
    }

    private static UniversalSpineItem Item(
        string title,
        UniversalSpineItemKind kind,
        UniversalSpineSignalStatus status,
        int priority,
        string sourceModule,
        string nextAction,
        string notes)
    {
        return new UniversalSpineItem
        {
            Title = title,
            Kind = kind,
            Status = status,
            Priority = priority,
            SourceModule = sourceModule,
            NextAction = nextAction,
            Notes = notes,
            ReviewDate = DateOnly.FromDateTime(DateTime.Today),
            UpdatedAt = DateTime.Now
        };
    }

    private static UniversalSpineLink Link(
        UniversalSpineItem from,
        UniversalSpineItem to,
        UniversalSpineLinkType linkType,
        string label,
        string notes)
    {
        return new UniversalSpineLink
        {
            FromItemId = from.Id,
            ToItemId = to.Id,
            LinkType = linkType,
            Label = label,
            Notes = notes,
            UpdatedAt = DateTime.Now
        };
    }
}
