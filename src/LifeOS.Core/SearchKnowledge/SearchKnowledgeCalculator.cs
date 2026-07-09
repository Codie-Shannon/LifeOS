namespace LifeOS.Core.SearchKnowledge;

public static class SearchKnowledgeCalculator
{
    public static SearchKnowledgeSummary Calculate(SearchKnowledgeProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var items = profile.Items ?? [];
        var profiles = profile.SearchProfiles ?? [];

        var review = items.Count(item => item.Status == KnowledgeItemStatus.ReviewNeeded);
        var planned = items.Count(item => item.Status == KnowledgeItemStatus.Planned);

        var reasons = new List<string>();

        if (profile.LocalIndexOnly)
        {
            reasons.Add("Search and knowledge are local-first; no external index or connector search is active in v3.5.");
        }

        if (profile.ManualReviewRequired)
        {
            reasons.Add("Manual review remains required before turning search hits or knowledge notes into final truth.");
        }

        if (!profile.ExternalSearchEnabled)
        {
            reasons.Add("External search remains off; v3.5 only proves the local search/knowledge shape.");
        }

        if (!profile.AiReasoningEnabled)
        {
            reasons.Add("AI reasoning is not active; the assistant layer comes later after the offline foundation is stable.");
        }

        if (!profile.IntegrationSourcesEnabled)
        {
            reasons.Add("Integration sources are not active; future v4 integrations will feed this structure rather than replace it.");
        }

        if (review > 0)
        {
            reasons.Add($"{review} knowledge item(s) need review before being treated as reliable.");
        }

        if (planned > 0)
        {
            reasons.Add($"{planned} knowledge item(s) are planned/future and should not be shown as complete.");
        }

        if (profiles.Count > 0)
        {
            reasons.Add($"{profiles.Count} local search profile(s) define how future search should be scoped.");
        }

        return new SearchKnowledgeSummary
        {
            Version = profile.Version,
            Mode = profile.Mode,
            TotalItems = items.Count,
            ActiveItems = items.Count(item => item.Status == KnowledgeItemStatus.Active),
            ReviewNeededItems = review,
            PlannedItems = planned,
            ProfileCount = profiles.Count,
            PinnedProfiles = profiles.Count(searchProfile => searchProfile.IsPinned),
            ManualProfiles = profiles.Count(searchProfile => searchProfile.IsManualOnly),
            SourceCount = items.Select(item => item.SourceType).Distinct().Count(),
            LocalIndexOnly = profile.LocalIndexOnly,
            ExternalSearchEnabled = profile.ExternalSearchEnabled,
            AiReasoningEnabled = profile.AiReasoningEnabled,
            IntegrationSourcesEnabled = profile.IntegrationSourcesEnabled,
            Reasons = reasons,
            PriorityItems = items
                .Where(item => item.Status is KnowledgeItemStatus.ReviewNeeded or KnowledgeItemStatus.Planned)
                .OrderBy(item => item.Priority)
                .ThenBy(item => item.Kind)
                .ToList(),
            RecentItems = items
                .OrderByDescending(item => item.UpdatedAt)
                .ThenBy(item => item.Priority)
                .Take(12)
                .ToList(),
            Profiles = profiles
                .OrderByDescending(searchProfile => searchProfile.IsPinned)
                .ThenBy(searchProfile => searchProfile.Name)
                .ToList()
        };
    }

    public static string FormatKind(KnowledgeItemKind kind)
    {
        return kind switch
        {
            KnowledgeItemKind.Note => "Note",
            KnowledgeItemKind.Decision => "Decision",
            KnowledgeItemKind.Proof => "Proof",
            KnowledgeItemKind.Release => "Release",
            KnowledgeItemKind.Module => "Module",
            KnowledgeItemKind.Workflow => "Workflow",
            KnowledgeItemKind.Boundary => "Boundary",
            KnowledgeItemKind.SearchProfile => "Search profile",
            _ => kind.ToString()
        };
    }

    public static string FormatStatus(KnowledgeItemStatus status)
    {
        return status switch
        {
            KnowledgeItemStatus.Active => "Active",
            KnowledgeItemStatus.ReviewNeeded => "Review needed",
            KnowledgeItemStatus.Planned => "Planned",
            KnowledgeItemStatus.Archived => "Archived",
            _ => status.ToString()
        };
    }

    public static string FormatSourceType(KnowledgeSourceType sourceType)
    {
        return sourceType switch
        {
            KnowledgeSourceType.LocalJson => "Local JSON",
            KnowledgeSourceType.ScreenshotDocs => "Screenshot docs",
            KnowledgeSourceType.ReleaseNotes => "Release notes",
            KnowledgeSourceType.EvidenceVault => "Evidence Vault",
            KnowledgeSourceType.UniversalSpine => "Universal Spine",
            KnowledgeSourceType.ManualEntry => "Manual entry",
            KnowledgeSourceType.FutureIntegration => "Future integration",
            _ => sourceType.ToString()
        };
    }
}
