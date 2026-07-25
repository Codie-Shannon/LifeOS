namespace LifeOS.Core.ProductCompletion;

public sealed record ProductLaneMetric(string Label, string Value, string Detail);

public sealed record ProductLaneAction(
    string Title,
    string State,
    string Detail,
    bool RequiresReview);

public sealed record ProductLaneDefinition(
    string Route,
    string ScreenshotGroup,
    string Title,
    string Subtitle,
    string Boundary,
    IReadOnlyList<ProductLaneMetric> Metrics,
    IReadOnlyList<ProductLaneAction> Actions);

public static class ProductLaneCatalog
{
    private static readonly Dictionary<string, ProductLaneDefinition> Lanes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["work-time"] = new(
                "work-time",
                "SG-67",
                "Work Time & Billable Records",
                "Groups 67-68 - timers, manual entries, daily totals and exportable source proof",
                "Local timers and manual records change only when the user acts. Evidence remains attached and reviewable.",
                new[]
                {
                    new ProductLaneMetric("Today", "6h 30m", "Local tracked time"),
                    new ProductLaneMetric("Billable", "5h 45m", "NZD 488.75"),
                    new ProductLaneMetric("Non-billable", "45m", "Administration"),
                    new ProductLaneMetric("Evidence", "7", "Source references retained")
                },
                new[]
                {
                    new ProductLaneAction("Website release preparation", "Running", "1h 12m - BayForge Systems - billable", false),
                    new ProductLaneAction("Client discovery call", "Completed", "1h 30m - evidence attached - ready for timesheet", false),
                    new ProductLaneAction("Correct manual entry", "Needs review", "45m manual adjustment with a required reason", true),
                    new ProductLaneAction("Export weekly timesheet", "Ready", "CSV preview contains billable classification and proof count", true)
                })
        };

    public static IReadOnlyCollection<string> Routes => Lanes.Keys;

    public static ProductLaneDefinition Get(string route) =>
        Lanes.TryGetValue(route, out ProductLaneDefinition? lane)
            ? lane
            : throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown product lane.");
}
