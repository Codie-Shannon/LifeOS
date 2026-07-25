namespace LifeOS.Core.GroceryLookup;

public enum NzRetailer
{
    NewWorld,
    PaknSave,
    Woolworths
}

public enum LookupMethod
{
    OfficialApi,
    BrowserAssisted,
    Manual
}

public enum LookupWorkerState
{
    Disabled,
    Ready,
    Running,
    NeedsReview,
    Failed
}

public sealed record GroceryLookupConsent(
    bool Enabled,
    IReadOnlySet<NzRetailer> Retailers,
    string Location,
    bool UseNearestTown,
    bool BackgroundRefresh,
    DateTimeOffset? GrantedAt);

public sealed record RetailerAdapter(
    NzRetailer Retailer,
    LookupMethod Method,
    string Limitation,
    bool Available);

public sealed record GroceryPriceEvidence(
    string Id,
    NzRetailer Retailer,
    string Product,
    decimal Price,
    string Unit,
    string Store,
    string SourceUrl,
    DateTimeOffset CapturedAt,
    int Confidence,
    LookupMethod Method);

public sealed record GroceryLookupResult(
    LookupWorkerState State,
    IReadOnlyList<GroceryPriceEvidence> Prices,
    string Status,
    bool CanMutateCart);

public sealed class GroceryLookupService
{
    public GroceryLookupResult Begin(
        GroceryLookupConsent consent,
        RetailerAdapter adapter,
        string product)
    {
        if (!consent.Enabled || consent.GrantedAt is null)
        {
            return new GroceryLookupResult(LookupWorkerState.Disabled, Array.Empty<GroceryPriceEvidence>(), "Consent is required.", false);
        }
        if (!consent.Retailers.Contains(adapter.Retailer))
        {
            return new GroceryLookupResult(LookupWorkerState.Disabled, Array.Empty<GroceryPriceEvidence>(), "Retailer is not permitted.", false);
        }
        if (!adapter.Available)
        {
            return new GroceryLookupResult(LookupWorkerState.Failed, Array.Empty<GroceryPriceEvidence>(), adapter.Limitation, false);
        }
        if (string.IsNullOrWhiteSpace(product))
        {
            throw new ArgumentException("A product is required.", nameof(product));
        }

        return new GroceryLookupResult(
            LookupWorkerState.Running,
            Array.Empty<GroceryPriceEvidence>(),
            $"Visible {adapter.Method} lookup running for {product.Trim()}.",
            false);
    }

    public GroceryLookupResult Complete(
        GroceryLookupResult running,
        IEnumerable<GroceryPriceEvidence> evidence)
    {
        if (running.State != LookupWorkerState.Running)
        {
            throw new InvalidOperationException("Only a running lookup can complete.");
        }

        GroceryPriceEvidence[] prices = evidence
            .Where(item => item.Price >= 0m)
            .OrderBy(item => item.Price)
            .ToArray();

        return new GroceryLookupResult(
            LookupWorkerState.NeedsReview,
            prices,
            prices.Length == 0 ? "No trusted prices returned; use manual entry." : $"{prices.Length} source-backed prices need review.",
            false);
    }

    public IReadOnlyList<GroceryPriceEvidence> SimilarProducts(
        IEnumerable<GroceryPriceEvidence> evidence,
        string product) =>
        evidence
            .Where(item => item.Product.Contains(product, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.Price)
            .ThenByDescending(item => item.Confidence)
            .ToArray();

    public bool IsFresh(GroceryPriceEvidence evidence, DateTimeOffset now, TimeSpan maximumAge)
    {
        if (maximumAge < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumAge), "Maximum age cannot be negative.");
        }

        TimeSpan age = now - evidence.CapturedAt;
        return age >= TimeSpan.Zero && age <= maximumAge;
    }
}
