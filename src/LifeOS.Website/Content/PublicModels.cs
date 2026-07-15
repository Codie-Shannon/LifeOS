namespace LifeOS.Website.Content;

public sealed record ProductStatus(string Name, string Version, string State, string Summary, string Route);
public sealed record AudiencePath(string Name, string Summary, string Route, string Signal);
public sealed record ProofMetric(string Value, string Label, string Detail);
public sealed record RouteEntry(string Path, string Title, string Description);

public sealed record DocumentationSection(string Heading, string[] Paragraphs);

public sealed record DocEntry(
    string Id,
    string Title,
    string Summary,
    string Audience,
    string Category,
    string Product,
    string Version,
    DateOnly Updated,
    string Route,
    string PublicStatus,
    string[] Keywords,
    DocumentationSection[] Sections,
    string[] RelatedIds,
    string[] EvidenceLinks);

public sealed record DocumentationInventoryItem(
    string Id,
    string Source,
    string CurrentLocation,
    string Classification,
    string IntendedRoute,
    string Audience,
    string Category,
    string Product,
    string Version,
    DateOnly LastReviewed,
    bool Public);
