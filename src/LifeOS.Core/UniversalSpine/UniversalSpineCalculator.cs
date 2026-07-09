namespace LifeOS.Core.UniversalSpine;

public static class UniversalSpineCalculator
{
    public static UniversalSpineSummary Calculate(UniversalSpineProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var items = profile.Items ?? [];
        var links = profile.Links ?? [];

        var active = items.Count(item => item.Status == UniversalSpineSignalStatus.Active);
        var review = items.Count(item => item.Status == UniversalSpineSignalStatus.ReviewNeeded);
        var blocked = items.Count(item => item.Status == UniversalSpineSignalStatus.Blocked);
        var waiting = items.Count(item => item.Status == UniversalSpineSignalStatus.Waiting);

        var reasons = new List<string>();

        if (profile.CrossModuleLinksEnabled)
        {
            reasons.Add("Cross-module links are enabled locally, so work, money, proof, relationships, daily flow, evidence, release state, and safety settings can be connected.");
        }

        if (profile.ManualReviewRequired)
        {
            reasons.Add("Manual review remains required before LifeOS treats connected state as final truth.");
        }

        if (profile.UniversalSearchPlanned)
        {
            reasons.Add("Universal search is planned, but v2.1 only builds the local spine and link model.");
        }

        if (profile.ExternalSyncEnabled)
        {
            reasons.Add("External sync is on. This should not be used for the offline foundation lane without review.");
        }
        else
        {
            reasons.Add("External sync remains off; v2.1 is still local-first.");
        }

        if (blocked > 0)
        {
            reasons.Add($"{blocked} spine item(s) are blocked and should be surfaced in the Command Centre.");
        }

        if (review > 0)
        {
            reasons.Add($"{review} spine item(s) need review before the link map can be treated as reliable.");
        }

        if (links.Count > 0)
        {
            reasons.Add($"{links.Count} cross-module link(s) are available for the local spine view.");
        }

        return new UniversalSpineSummary
        {
            Version = profile.Version,
            SpineMode = profile.SpineMode,
            TotalItems = items.Count,
            ActiveItems = active,
            ReviewNeededItems = review,
            BlockedItems = blocked,
            WaitingItems = waiting,
            LinkCount = links.Count,
            NeedsProofLinks = links.Count(link => link.LinkType == UniversalSpineLinkType.NeedsProof),
            FollowUpLinks = links.Count(link => link.LinkType == UniversalSpineLinkType.NeedsFollowUp),
            MoneyLinks = links.Count(link => link.LinkType == UniversalSpineLinkType.ProducesMoney),
            ModuleCount = items.Select(item => item.SourceModule).Where(module => !string.IsNullOrWhiteSpace(module)).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            ExternalSyncEnabled = profile.ExternalSyncEnabled,
            Reasons = reasons,
            PriorityItems = items
                .Where(item => item.Status is UniversalSpineSignalStatus.Blocked or UniversalSpineSignalStatus.ReviewNeeded or UniversalSpineSignalStatus.Waiting)
                .OrderBy(item => item.Priority)
                .ThenBy(item => item.Kind)
                .ToList(),
            RecentItems = items
                .OrderByDescending(item => item.UpdatedAt)
                .ThenBy(item => item.Priority)
                .Take(12)
                .ToList(),
            Links = links
                .OrderBy(link => link.LinkType)
                .ThenBy(link => link.Label)
                .ToList()
        };
    }

    public static string FormatKind(UniversalSpineItemKind kind)
    {
        return kind switch
        {
            UniversalSpineItemKind.Work => "Work",
            UniversalSpineItemKind.Money => "Money",
            UniversalSpineItemKind.Proof => "Proof",
            UniversalSpineItemKind.Relationship => "Relationship",
            UniversalSpineItemKind.DailyFlow => "Daily flow",
            UniversalSpineItemKind.Evidence => "Evidence",
            UniversalSpineItemKind.Release => "Release",
            UniversalSpineItemKind.Settings => "Settings",
            UniversalSpineItemKind.Knowledge => "Knowledge",
            _ => kind.ToString()
        };
    }

    public static string FormatStatus(UniversalSpineSignalStatus status)
    {
        return status switch
        {
            UniversalSpineSignalStatus.Active => "Active",
            UniversalSpineSignalStatus.Waiting => "Waiting",
            UniversalSpineSignalStatus.ReviewNeeded => "Review needed",
            UniversalSpineSignalStatus.Blocked => "Blocked",
            UniversalSpineSignalStatus.Complete => "Complete",
            UniversalSpineSignalStatus.Archived => "Archived",
            _ => status.ToString()
        };
    }

    public static string FormatLinkType(UniversalSpineLinkType linkType)
    {
        return linkType switch
        {
            UniversalSpineLinkType.Supports => "Supports",
            UniversalSpineLinkType.Blocks => "Blocks",
            UniversalSpineLinkType.NeedsProof => "Needs proof",
            UniversalSpineLinkType.NeedsFollowUp => "Needs follow-up",
            UniversalSpineLinkType.ProducesMoney => "Produces money",
            UniversalSpineLinkType.RelatesTo => "Relates to",
            UniversalSpineLinkType.Supersedes => "Supersedes",
            UniversalSpineLinkType.Documents => "Documents",
            _ => linkType.ToString()
        };
    }
}
