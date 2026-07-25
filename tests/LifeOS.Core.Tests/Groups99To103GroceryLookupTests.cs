using LifeOS.Core.GroceryLookup;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups99To103GroceryLookupTests
{
    private readonly GroceryLookupService _service = new();
    private readonly DateTimeOffset _now = new(2026, 7, 27, 12, 0, 0, TimeSpan.FromHours(12));

    [Fact]
    public void Lookup_cannot_start_without_explicit_consent()
    {
        GroceryLookupResult result = _service.Begin(
            Consent(false),
            Adapter(NzRetailer.NewWorld),
            "milk");

        Assert.Equal(LookupWorkerState.Disabled, result.State);
        Assert.False(result.CanMutateCart);
    }

    [Fact]
    public void Retailer_permission_is_enforced_per_store()
    {
        GroceryLookupResult result = _service.Begin(
            Consent(true),
            Adapter(NzRetailer.Woolworths),
            "bread");

        Assert.Equal(LookupWorkerState.Disabled, result.State);
        Assert.Contains("not permitted", result.Status);
    }

    [Fact]
    public void Completed_prices_retain_source_freshness_and_confidence()
    {
        GroceryLookupResult running = _service.Begin(Consent(true), Adapter(NzRetailer.NewWorld), "milk");
        GroceryPriceEvidence evidence = Price("Standard milk 2L", 4.20m, NzRetailer.NewWorld, 88);

        GroceryLookupResult completed = _service.Complete(running, new[] { evidence });

        Assert.Equal(LookupWorkerState.NeedsReview, completed.State);
        Assert.Equal("https://example.invalid/new-world/milk", completed.Prices[0].SourceUrl);
        Assert.True(_service.IsFresh(evidence, _now, TimeSpan.FromDays(1)));
        Assert.False(completed.CanMutateCart);
    }

    [Fact]
    public void Similar_product_comparison_orders_by_price_then_confidence()
    {
        IReadOnlyList<GroceryPriceEvidence> prices = _service.SimilarProducts(new[]
        {
            Price("Long grain rice 1kg", 4.50m, NzRetailer.NewWorld, 90),
            Price("Long grain rice 1kg", 3.90m, NzRetailer.PaknSave, 75),
            Price("Bread loaf", 2.90m, NzRetailer.NewWorld, 95)
        }, "rice");

        Assert.Equal(2, prices.Count);
        Assert.Equal(NzRetailer.PaknSave, prices[0].Retailer);
    }

    [Fact]
    public void Future_dated_price_evidence_is_not_treated_as_fresh()
    {
        GroceryPriceEvidence futureEvidence = Price("Standard milk 2L", 4.20m, NzRetailer.NewWorld, 88) with
        {
            CapturedAt = _now.AddMinutes(5)
        };

        Assert.False(_service.IsFresh(futureEvidence, _now, TimeSpan.FromDays(1)));
    }

    private GroceryLookupConsent Consent(bool enabled) =>
        new(
            enabled,
            new HashSet<NzRetailer> { NzRetailer.NewWorld, NzRetailer.PaknSave },
            "Whanganui",
            true,
            false,
            enabled ? _now : null);

    private static RetailerAdapter Adapter(NzRetailer retailer) =>
        new(retailer, LookupMethod.BrowserAssisted, "Transparent website lookup.", true);

    private GroceryPriceEvidence Price(string product, decimal price, NzRetailer retailer, int confidence) =>
        new(
            Guid.NewGuid().ToString("N"),
            retailer,
            product,
            price,
            "each",
            "Whanganui",
            $"https://example.invalid/{retailer.ToString().Replace("PaknSave", "paknsave", StringComparison.Ordinal).Replace("NewWorld", "new-world", StringComparison.Ordinal).ToLowerInvariant()}/milk",
            _now,
            confidence,
            LookupMethod.BrowserAssisted);
}
