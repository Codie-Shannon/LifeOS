namespace LifeOS.Website.Content;

public static class PublicContent
{
    public const string PrimaryMessage = "A local-first personal operating system that turns work, money, projects, evidence and daily pressure into visible, reviewable state.";
    public static readonly DateOnly LastUpdated = new(2026, 7, 15);

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
        new("187", "Core + Desktop tests", "Existing v7 baseline"),
        new("34", "Companion tests", "Existing beta baseline"),
        new("Release", "Build discipline", "Desktop and Android verified"),
        new("Clean", "Security posture", "Vulnerability and secret scans")
    ];

    public static IReadOnlyList<DocEntry> Docs { get; } =
    [
        new("Start with LifeOS", "Understand the product split, local-first boundary and current release state.", "Users", "Guide", "Platform", "v8", LastUpdated, "/docs/guides", ["start", "overview", "local-first"]),
        new("Desktop operating model", "How Desktop turns pressure into explicit state and reviewable decisions.", "Users", "Concept", "Desktop", "v7 beta", LastUpdated, "/docs/concepts", ["desktop", "state", "review"]),
        new("Mobile Companion boundary", "What the lightweight Companion does and deliberately does not do.", "Users", "Concept", "Companion", "v0.1 beta", LastUpdated, "/docs/concepts", ["mobile", "companion", "capture"]),
        new("Safety and trust model", "Local-first storage, review-first actions and fail-closed boundaries.", "All", "Concept", "Platform", "Current", LastUpdated, "/safety", ["safety", "trust", "privacy"]),
        new("Module map", "A structured landing for current and future module documentation.", "Users", "Module", "Desktop", "v7 beta", LastUpdated, "/docs/modules", ["module", "map", "desktop"]),
        new("Release history", "Public release summaries and validation checkpoints.", "Technical", "Release", "Platform", "Current", LastUpdated, "/docs/releases", ["release", "version", "validation"]),
        new("Architecture review", "Product boundaries and the static-first Website foundation.", "Technical", "Guide", "Website", "v0.1 alpha", LastUpdated, "/solutions/technical-reviewers", ["architecture", "blazor", "website"])
    ];

    public static IReadOnlyList<RouteEntry> Routes { get; } =
    [
        new("/", "Home", "LifeOS public front door"), new("/product", "Product", "Product overview"),
        new("/product/desktop", "Desktop", "Desktop product"), new("/product/companion", "Mobile Companion", "Companion product"),
        new("/solutions", "Solutions", "Audience landing"), new("/solutions/businesses", "For Businesses", "Business positioning"),
        new("/solutions/individuals", "For Individuals", "Individual positioning"), new("/solutions/technical-reviewers", "For Technical Reviewers", "Technical review"),
        new("/safety", "Safety and Trust", "Safety model"), new("/docs", "Documentation", "Searchable docs front page"),
        new("/docs/guides", "Guides", "Guides landing"), new("/docs/concepts", "Concepts", "Concepts landing"),
        new("/docs/modules", "Modules", "Modules landing"), new("/docs/releases", "Releases", "Releases landing"),
        new("/evidence", "Evidence", "Development proof"), new("/roadmap", "Roadmap", "Product tracks"),
        new("/access", "Early Access", "Preview waitlist boundary"), new("/about", "About", "Product and creator"),
        new("/privacy", "Privacy", "Website privacy boundary")
    ];
}
