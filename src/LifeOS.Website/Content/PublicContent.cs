namespace LifeOS.Website.Content;

public static class PublicContent
{
    public const string PrimaryMessage = "A local-first personal operating system that turns work, money, projects, evidence and daily pressure into visible, reviewable state.";
    public static readonly DateOnly LastUpdated = new(2026, 7, 16);

    public static IReadOnlyList<ProductStatus> Products { get; } =
    [
        new("Desktop", "v7.0.0-beta.1", "Beta complete", "The full local operating environment for planning, review, evidence and guarded execution.", "/product/desktop"),
        new("Mobile Companion", "v0.1.0-beta.1", "Beta complete", "A deliberately lightweight companion for capture, glance, review, outbox and send-to-desktop.", "/product/companion"),
        new("Full Mobile", "Future track", "Planned", "A separate broader mobile application. It is not part of the current release.", "/roadmap")
    ];

    public static IReadOnlyList<AudiencePath> Audiences { get; } =
    [
        new("For Businesses", "See workflow clarity, evidence trails and guarded internal-system thinking.", "/solutions/businesses", "Control without hidden automation"),
        new("For Individuals", "Bring work, money, projects and follow-ups into one visible operating state.", "/solutions/individuals", "Own the state locally"),
        new("For Technical Reviewers", "Inspect architecture, boundaries, validation, releases and defects fixed.", "/solutions/technical-reviewers", "Evidence over claims")
    ];

    public static IReadOnlyList<ProofMetric> ProofMetrics { get; } =
    [
        new("22", "Website tests", "Documentation and launch-readiness baseline"),
        new("187", "Core + Desktop tests", "Existing v7 baseline"),
        new("34", "Companion tests", "Existing beta baseline"),
        new("Clean", "Security posture", "Release, vulnerability and secret scans")
    ];

    private static DocumentationSection S(string heading, params string[] paragraphs) => new(heading, paragraphs);

    public static IReadOnlyList<DocEntry> Docs { get; } =
    [
        new("getting-started", "Getting Started", "Understand LifeOS purpose, the product split and the safest first steps.", "User", "Guide", "Platform", "v8 / Website v0.3 beta", LastUpdated, "/docs/guides/getting-started", "Public",
            ["start", "overview", "onboarding", "local-first", "product split"],
            [S("What LifeOS is", PrimaryMessage), S("Choose the right product", "Desktop is the full local operating environment. Mobile Companion is intentionally lightweight. Full Mobile is a separate planned application."), S("Start safely", "Review current state before changing it, keep evidence attached to important decisions and treat unavailable states as real boundaries rather than invitations to bypass them.")],
            ["desktop-overview", "product-release-boundaries", "operating-philosophy"], []),

        new("desktop-overview", "Desktop Overview", "Major Desktop workspaces and the current v7 beta boundary.", "User", "Module", "Desktop", "v7.0.0-beta.1", LastUpdated, "/docs/modules/desktop", "Public",
            ["desktop", "workspaces", "command centre", "money", "projects", "follow-ups"],
            [S("Operating environment", "Desktop is the full local LifeOS environment for planning, review, evidence and guarded execution."), S("Major workspaces", "Command Centre, work, money, projects, follow-ups, evidence, integrations, automation review and Assistant planning expose visible state without pretending every action is automatic."), S("Current boundary", "Desktop remains usable offline and does not require the public Website to perform immediate work or show safety-critical guidance.")],
            ["assistant", "controlled-automation", "evidence-provenance"], ["/evidence"]),

        new("mobile-companion-guide", "Mobile Companion Guide", "Capture, pairing, glance, outbox, review and offline behaviour.", "User", "Guide", "Companion", "v0.1.0-beta.1", LastUpdated, "/docs/guides/mobile-companion", "Public",
            ["mobile", "companion", "capture", "pairing", "outbox", "offline"],
            [S("Purpose", "Mobile Companion is a separate lightweight application for capture, glance, review, outbox and send-to-desktop."), S("Pairing and transfer", "Pairing establishes a bounded device relationship. Outbox items remain visible until transfer succeeds or the user resolves the failure."), S("Offline behaviour", "Capture and review remain honest about stale or unavailable state. The Companion does not silently invent synchronization success.")],
            ["product-release-boundaries", "troubleshooting-recovery"], ["/product/companion", "/evidence"]),

        new("mobile-companion-module", "Mobile Companion", "The lightweight Companion module boundary, capture flow, outbox and offline behaviour.", "User", "Module", "Companion", "v0.1.0-beta.1", LastUpdated, "/docs/modules/mobile-companion", "Public",
            ["mobile", "companion", "module", "capture", "glance", "review", "outbox", "send-to-desktop", "offline"],
            [S("Lightweight by design", "Mobile Companion is a separate intentionally lightweight application for capture, glance, review, outbox and send-to-desktop. It is not the future Full Mobile application."), S("Visible transfer state", "Captured items and outbox work remain visible until transfer succeeds or the user resolves the failure. The Companion never silently invents synchronization success."), S("Offline boundary", "Capture and review remain available within the local Companion boundary while stale or unavailable state is labelled honestly.")],
            ["mobile-companion-guide", "product-release-boundaries", "troubleshooting-recovery"], ["/product/companion", "/evidence"]),

        new("operating-philosophy", "Local-first, Review-first and Fail-closed", "Plain-language operating philosophy and trust boundaries.", "All", "Concept", "Platform", "Current", LastUpdated, "/docs/concepts/operating-philosophy", "Public",
            ["local-first", "review-first", "fail-closed", "safety", "trust"],
            [S("Local-first", "Private Desktop and Companion records stay local unless a separately approved feature explicitly moves them."), S("Review-first", "Consequential actions should remain inspectable and confirmable. Authentication or data access never implies permission to act."), S("Fail-closed", "When a real handling path is unavailable, LifeOS shows the boundary instead of pretending success or silently dropping work.")],
            ["controlled-automation", "evidence-provenance", "troubleshooting-recovery"], ["/safety"]),

        new("evidence-provenance", "Evidence and Provenance", "How LifeOS represents sources, trust, review and audit trails.", "Business evaluator", "Concept", "Platform", "Current", LastUpdated, "/docs/concepts/evidence-provenance", "Public",
            ["evidence", "provenance", "source", "trust", "audit"],
            [S("Evidence over claims", "Important product claims should point to release state, tests, screenshots or review records."), S("Provenance", "Source-backed records retain enough origin context for a reviewer to understand where information came from and what has changed."), S("Public proof", "Website examples use fictional or sanitized identities. Private client data, raw logs and local records are excluded from public output.")],
            ["assistant", "controlled-automation"], ["/evidence", "/docs/releases/group-41"]),

        new("integrations", "Integrations", "Read-only connectors, review inbox and safe disconnect behaviour.", "Technical reviewer", "Module", "Desktop", "v7 beta", LastUpdated, "/docs/modules/integrations", "Public",
            ["integration", "connector", "read-only", "review inbox", "disconnect"],
            [S("Read-only first", "Calendar, Gmail and local imports enter LifeOS through bounded read-only paths before any future write capability is considered."), S("Review Inbox", "Imported candidates remain separate from operational records until reviewed."), S("Disconnect safely", "Disconnecting a source stops future access without rewriting historical proof or pretending that previously imported records never existed.")],
            ["controlled-automation", "evidence-provenance", "troubleshooting-recovery"], []),

        new("controlled-automation", "Controlled Automation", "Proposal, approval, final confirmation, execution gates, rollback and Emergency Stop.", "Technical reviewer", "Module", "Desktop", "v7 beta", LastUpdated, "/docs/modules/automation", "Public",
            ["automation", "approval", "execution gate", "rollback", "emergency stop"],
            [S("Proposal before execution", "Automation begins as a visible proposal with scope, dependencies, evidence and expected effects."), S("Approval and confirmation", "Approval is not execution. A final confirmation and readiness gate remain required for consequential work."), S("Containment", "Rollback and Emergency Stop exist to contain failure. No automation receives unbounded autonomous authority.")],
            ["assistant", "operating-philosophy", "evidence-provenance"], ["/evidence"]),

        new("assistant", "Assistant", "Source-backed answers, review-only planning, explicit memory and a no-execution boundary.", "User", "Module", "Desktop", "v7.0.0-beta.1", LastUpdated, "/docs/modules/assistant", "Public",
            ["assistant", "source-backed", "planning", "memory", "no execution"],
            [S("Source-backed answers", "The Assistant searches only enabled read-only sources and separates facts, assumptions, gaps and conflicts."), S("Review-only planning", "Answers can become editable planning blocks and one controlled review artifact. They do not become tasks, payments, messages or automation runs automatically."), S("Explicit memory", "Durable memory is created only through explicit review and confirmation. The Assistant cannot execute tools, scripts or external writes.")],
            ["controlled-automation", "evidence-provenance", "desktop-overview"], ["/evidence"]),

        new("troubleshooting-recovery", "Troubleshooting and Recovery", "Safe restart, offline or stale state and malformed local-state recovery.", "User", "Troubleshooting", "Platform", "Current", LastUpdated, "/docs/guides/troubleshooting-recovery", "Public",
            ["troubleshooting", "recovery", "offline", "stale", "malformed state"],
            [S("Start with visible state", "Capture the current error and preserve evidence before resetting anything."), S("Offline or stale", "Keep local work available, label stale data clearly and retry only through the normal bounded path."), S("Malformed local state", "Prefer validated backups and narrow recovery tools. Do not delete unknown files or bypass safety checks merely to make the error disappear.")],
            ["operating-philosophy", "mobile-companion-guide"], []),

        new("product-release-boundaries", "Product and Release Boundaries", "Desktop, Companion, Full Mobile, Website and future Portal separation.", "Business evaluator", "Release", "Platform", "Website v0.2", LastUpdated, "/docs/releases/product-boundaries", "Public",
            ["product boundary", "release", "desktop", "companion", "mobile", "website", "portal"],
            [S("Current products", "Desktop v7 beta and Mobile Companion beta are complete baselines. Website v0.2 is the public knowledge foundation."), S("Planned products", "Full Mobile is a separate planned application. A future Portal may support selected secure web capabilities only after Desktop and Full Mobile stabilize."), S("Not included", "The public Website has no login, account model, billing, licence system, downloads or private dashboard.")],
            ["getting-started", "mobile-companion-guide", "group-41-release"], ["/roadmap"]),

        new("group-41-release", "Group 41 Documentation Foundation", "The first real public documentation set, deterministic search and controlled Desktop link-outs.", "Technical reviewer", "Release", "Website", "v0.3.0-beta.1", LastUpdated, "/docs/releases/group-41", "Public",
            ["group 41", "website", "documentation", "release", "validation"],
            [S("Delivered", "A controlled inventory, ten public documents, stable routes, metadata, breadcrumbs, related content and deterministic local search."), S("Desktop boundary", "Only selected long-form Assistant copy is reduced, with action-level safety retained and an allowlisted Open Documentation link."), S("Validation", "Website, Core/Desktop and Companion tests remain separate. Static output is checked for private paths, tracking scripts and forbidden content patterns.")],
            ["product-release-boundaries", "evidence-provenance"], ["/evidence"]),

        new("release-history", "Release history", "Public release summaries and validation checkpoints.", "User", "Release", "Platform", "Current", LastUpdated, "/docs/releases/release-history", "Public",
            ["release", "history", "versions", "validation", "checkpoint"],
            [S("Release discipline", "LifeOS records completed release groups, validation totals and product boundaries without presenting planned work as available."), S("Current public baseline", "Desktop v7 beta and Mobile Companion beta are complete. Website v0.3 beta is the public launch-readiness checkpoint."), S("Evidence links", "Release claims link to sanitized screenshots, tests and repository-backed validation where practical.")],
            ["group-41-release", "product-release-boundaries"], ["/evidence", "/roadmap"]),

        new("desktop-operating-model", "Desktop operating model", "How Desktop turns pressure into explicit state and reviewable decisions.", "User", "Concept", "Desktop", "v7.0.0-beta.1", LastUpdated, "/docs/concepts/desktop-operating-model", "Public",
            ["desktop state", "operating model", "visible state", "reviewable decisions", "pressure"],
            [S("Visible state", "Desktop turns work, money, projects, evidence and follow-ups into explicit reviewable state."), S("Review boundaries", "Consequential changes remain visible and confirmable rather than being hidden behind broad automation."), S("Offline boundary", "Desktop remains usable without the public Website and retains immediate warnings and recovery guidance locally.")],
            ["desktop-overview", "operating-philosophy"], ["/product/desktop"]),

        new("safety-trust-model", "Safety and trust model", "Local-first storage, review-first actions and fail-closed boundaries.", "All", "Concept", "Platform", "Current", LastUpdated, "/docs/concepts/safety-trust-model", "Public",
            ["safety", "trust", "local-first", "review-first", "fail-closed"],
            [S("Local-first", "Private Desktop and Companion records are not published by the Website."), S("Review-first", "Important actions remain inspectable and require clear confirmation."), S("Fail-closed", "Unavailable handling paths remain visibly unavailable instead of pretending success.")],
            ["operating-philosophy", "controlled-automation"], ["/safety"])
    ];

    public static IReadOnlyList<DocumentationInventoryItem> DocumentationInventory { get; } =
    [
        new("getting-started", "Group 40 product content", "Website product pages", "Move to Website", "/docs/guides/getting-started", "User", "Guide", "Platform", "v8", LastUpdated, true),
        new("desktop-overview", "Desktop module summaries", "Desktop contextual surfaces", "Summary only", "/docs/modules/desktop", "User", "Module", "Desktop", "v7 beta", LastUpdated, true),
        new("mobile-companion-guide", "Companion release proof", "Repository release records", "Move to Website", "/docs/guides/mobile-companion", "User", "Guide", "Companion", "v0.1 beta", LastUpdated, true),
        new("mobile-companion-module", "Companion product boundary", "Companion module documentation", "Move to Website", "/docs/modules/mobile-companion", "User", "Module", "Companion", "v0.1 beta", LastUpdated, true),
        new("operating-philosophy", "Safety and trust model", "Website safety page", "Move to Website", "/docs/concepts/operating-philosophy", "All", "Concept", "Platform", "Current", LastUpdated, true),
        new("evidence-provenance", "Evidence architecture", "Repository evidence records", "Move to Website", "/docs/concepts/evidence-provenance", "Business evaluator", "Concept", "Platform", "Current", LastUpdated, true),
        new("integrations", "Integration boundary copy", "Desktop Integration Inbox", "Summary only", "/docs/modules/integrations", "Technical reviewer", "Module", "Desktop", "v7 beta", LastUpdated, true),
        new("controlled-automation", "Automation safety copy", "Desktop Automation", "Summary only", "/docs/modules/automation", "Technical reviewer", "Module", "Desktop", "v7 beta", LastUpdated, true),
        new("assistant", "Assistant boundary copy", "Desktop Assistant planning", "Summary only", "/docs/modules/assistant", "User", "Module", "Desktop", "v7 beta", LastUpdated, true),
        new("troubleshooting-recovery", "Recovery guidance", "Desktop immediate error surfaces", "Keep in Desktop", "/docs/guides/troubleshooting-recovery", "User", "Troubleshooting", "Platform", "Current", LastUpdated, true),
        new("product-release-boundaries", "Release state", "Version and roadmap records", "Move to Website", "/docs/releases/product-boundaries", "Business evaluator", "Release", "Platform", "Current", LastUpdated, true)
    ];

    public static IReadOnlyList<RouteEntry> Routes { get; } =
    new RouteEntry[]
    {
        new("/", "Home", "LifeOS public front door"), new("/product", "Product", "Product overview"),
        new("/product/desktop", "Desktop", "Desktop product"), new("/product/companion", "Mobile Companion", "Companion product"),
        new("/solutions", "Solutions", "Audience landing"), new("/solutions/businesses", "For Businesses", "Business positioning"),
        new("/solutions/individuals", "For Individuals", "Individual positioning"), new("/solutions/technical-reviewers", "For Technical Reviewers", "Technical review"),
        new("/safety", "Safety and Trust", "Safety model"), new("/docs", "Documentation", "Searchable docs front page"),
        new("/docs/guides", "Guides", "Guides landing"), new("/docs/concepts", "Concepts", "Concepts landing"),
        new("/docs/modules", "Modules", "Modules landing"), new("/docs/releases", "Releases", "Releases landing"),
        new("/evidence", "Evidence", "Development proof"), new("/roadmap", "Roadmap", "Product tracks"),
        new("/access", "Early Access", "Preview waitlist boundary"), new("/about", "About", "Product and creator"),
        new("/privacy", "Privacy", "Website privacy boundary"), new("/downloads", "Downloads", "Future GitHub Releases boundary")
    }.Concat(Docs.Select(x => new RouteEntry(x.Route, x.Title, x.Summary))).ToArray();
}
