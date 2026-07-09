using LifeOS.Core.SearchKnowledge;

namespace LifeOS.Shared.SearchKnowledge;

public static class SearchKnowledgeDemoData
{
    public static SearchKnowledgeProfile CreateDefaultProfile()
    {
        return new SearchKnowledgeProfile
        {
            Version = "v3.5",
            Mode = "Local advanced search and knowledge foundation",
            LocalIndexOnly = true,
            ManualReviewRequired = true,
            ExternalSearchEnabled = false,
            AiReasoningEnabled = false,
            IntegrationSourcesEnabled = false,
            Notes = "Local knowledge and search profile foundation before integrations and AI assistant layers.",
            Items =
            [
                Item("v3.0 OS navigation boundary", KnowledgeItemKind.Boundary, KnowledgeItemStatus.Active, KnowledgeSourceType.ReleaseNotes, 1, "OS Navigation", "v3.0 strengthens navigation but is not the major workspace redesign.", "v3.0 OS navigation core modules workspace groups boundary deferred UI reshape"),
                Item("Universal Spine links module context", KnowledgeItemKind.Decision, KnowledgeItemStatus.Active, KnowledgeSourceType.UniversalSpine, 1, "Universal Spine", "The spine links work, money, proof, relationships, daily flow, release state, safety, and knowledge.", "universal spine cross module links proof money relationship daily release settings knowledge"),
                Item("Expected money is not safe", KnowledgeItemKind.Workflow, KnowledgeItemStatus.Active, KnowledgeSourceType.LocalJson, 1, "Money Pressure", "Expected and pending money remain separate from safe-to-spend until confirmed paid.", "expected money not safe pending manual income paid work invoice readiness"),
                Item("Screenshot privacy rule", KnowledgeItemKind.Boundary, KnowledgeItemStatus.Active, KnowledgeSourceType.ScreenshotDocs, 1, "Settings / Safety", "Docs and screenshots must avoid real names, emails, URLs, tenant IDs, secrets, and payment details.", "screenshot privacy demo safe no private identifiers no secrets"),
                Item("Relationship Radar does not read real inboxes", KnowledgeItemKind.Boundary, KnowledgeItemStatus.Active, KnowledgeSourceType.ReleaseNotes, 2, "Relationship Radar", "Relationship Radar proves waiting-on and follow-up state using fictional/local profiles before integrations.", "relationship radar waiting on do not chase follow up local profiles no inbox"),
                Item("Desktop Release Centre controls release readiness", KnowledgeItemKind.Module, KnowledgeItemStatus.Active, KnowledgeSourceType.ReleaseNotes, 2, "Desktop Release", "Release readiness tracks screenshots, docs, local-only state, review gates, and planned future release work.", "desktop release readiness screenshots docs tag local only review gates"),
                Item("Advanced Search is local-only in v3.5", KnowledgeItemKind.SearchProfile, KnowledgeItemStatus.ReviewNeeded, KnowledgeSourceType.ManualEntry, 1, "Search / Knowledge", "v3.5 adds search profiles and knowledge items but does not add external indexing or AI reasoning.", "advanced search local only no external index no AI reasoning"),
                Item("Knowledge Base planned after local shape", KnowledgeItemKind.Module, KnowledgeItemStatus.Planned, KnowledgeSourceType.FutureIntegration, 3, "Search / Knowledge", "The knowledge layer is structured now so future integrations can feed it later.", "knowledge base future integrations local shape"),
                Item("Integration source map", KnowledgeItemKind.Decision, KnowledgeItemStatus.Planned, KnowledgeSourceType.FutureIntegration, 3, "Search / Knowledge", "Future v4 integrations should feed modules and the Universal Spine instead of bypassing them.", "v4 integrations source map modules universal spine search knowledge")
            ],
            SearchProfiles =
            [
                Profile("Release and screenshot history", "release screenshot tag docs version history", "Release notes, screenshot groups, README, current status", true, "Use to find what changed in each staged group."),
                Profile("Money and proof safety", "expected money proof invoice readiness paid work safe to spend", "Money Pressure, Paid Work Centre, Proof Tracker, Evidence Vault", true, "Use before treating any work as invoice-ready or safe money."),
                Profile("Relationship and follow-up state", "waiting on follow up do not chase relationship client safe wording", "Relationship Radar, Follow-Ups, Universal Spine", true, "Use before chasing or drafting manual wording."),
                Profile("Daily operating control", "daily flow stop points low energy next safe action agenda", "Daily Operating Flow, Agenda, Daily State", false, "Use to decide what moves today."),
                Profile("Future integration landing zones", "outlook gmail calendar files accounting timer integrations landing zones", "Universal Spine, OS Navigation, Search / Knowledge", false, "Use to map future v4 integration inputs.")
            ]
        };
    }

    private static KnowledgeItem Item(
        string title,
        KnowledgeItemKind kind,
        KnowledgeItemStatus status,
        KnowledgeSourceType sourceType,
        int priority,
        string sourceModule,
        string summary,
        string searchText)
    {
        return new KnowledgeItem
        {
            Title = title,
            Kind = kind,
            Status = status,
            SourceType = sourceType,
            Priority = priority,
            SourceModule = sourceModule,
            Summary = summary,
            SearchText = searchText,
            NextAction = status == KnowledgeItemStatus.Active ? "Keep available in local search/knowledge view." : "Review before treating as complete.",
            ReviewDate = DateOnly.FromDateTime(DateTime.Today),
            UpdatedAt = DateTime.Now
        };
    }

    private static KnowledgeSearchProfile Profile(
        string name,
        string queryHint,
        string scope,
        bool pinned,
        string notes)
    {
        return new KnowledgeSearchProfile
        {
            Name = name,
            QueryHint = queryHint,
            Scope = scope,
            IsPinned = pinned,
            IsManualOnly = true,
            UsesExternalIndex = false,
            Notes = notes,
            UpdatedAt = DateTime.Now
        };
    }
}
