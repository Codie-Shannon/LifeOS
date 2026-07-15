namespace LifeOS.Website.Content;

public sealed record ProductStatus(string Name, string Version, string State, string Summary, string Route);
public sealed record AudiencePath(string Name, string Summary, string Route, string Signal);
public sealed record DocEntry(string Title, string Summary, string Audience, string Category, string Product, string Version, DateOnly Updated, string Route, string[] Keywords);
public sealed record RouteEntry(string Path, string Title, string Description);
public sealed record ProofMetric(string Value, string Label, string Detail);
