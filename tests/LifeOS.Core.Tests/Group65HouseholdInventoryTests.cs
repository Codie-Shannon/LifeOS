using LifeOS.Core.Grocery;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Group65HouseholdInventoryTests
{
    private readonly HouseholdInventoryService _service = new();

    [Fact]
    public void Inventory_dashboard_keeps_all_stock_states_visible()
    {
        var data = HouseholdInventoryProofData.Build();

        var dashboard = _service.BuildDashboard(
            data.Inventory,
            data.Stores,
            data.Recipes,
            new DateOnly(2026, 7, 22));

        Assert.Equal(1, dashboard.OutCount);
        Assert.Equal(1, dashboard.LowCount);
        Assert.Equal(1, dashboard.EnoughCount);
        Assert.Equal(1, dashboard.OverstockedCount);
        Assert.Equal(1, dashboard.UnknownCount);
    }

    [Fact]
    public void Out_low_and_unknown_stock_become_review_candidates_only()
    {
        var data = HouseholdInventoryProofData.Build();

        var dashboard = _service.BuildDashboard(
            data.Inventory,
            data.Stores,
            data.Recipes,
            new DateOnly(2026, 7, 22));

        var groceryCandidates = _service.BuildGroceryReviewCandidates(dashboard);

        Assert.Contains(groceryCandidates, candidate => candidate.Title == "Milk");
        Assert.Contains(groceryCandidates, candidate => candidate.Title == "Bread");
        Assert.Contains(groceryCandidates, candidate => candidate.Title == "Dishwasher tablets");
        Assert.All(groceryCandidates, candidate => Assert.True(candidate.RequiresReview));
    }

    [Fact]
    public void Overstocked_items_do_not_create_buy_candidates()
    {
        var data = HouseholdInventoryProofData.Build();

        var dashboard = _service.BuildDashboard(
            data.Inventory,
            data.Stores,
            data.Recipes,
            new DateOnly(2026, 7, 22));

        var groceryCandidates = _service.BuildGroceryReviewCandidates(dashboard);

        Assert.DoesNotContain(groceryCandidates, candidate => candidate.Title == "Rice");
    }

    [Fact]
    public void Recipe_ingredient_gaps_are_review_first()
    {
        var data = HouseholdInventoryProofData.Build();

        var dashboard = _service.BuildDashboard(
            data.Inventory,
            data.Stores,
            data.Recipes,
            new DateOnly(2026, 7, 22));

        Assert.Contains(
            dashboard.ReviewCandidates,
            candidate =>
                candidate.Kind == HouseholdInventoryCandidateKind.RecipeIngredientGap &&
                candidate.Title == "Rice bowl: Sauce" &&
                candidate.RequiresReview);
    }

    [Fact]
    public void Store_profiles_are_price_context_not_trusted_feeds()
    {
        var data = HouseholdInventoryProofData.Build();

        var dashboard = _service.BuildDashboard(
            data.Inventory,
            data.Stores,
            data.Recipes,
            new DateOnly(2026, 7, 22));

        Assert.All(dashboard.StoreProfiles, store => Assert.True(store.PriceContextOnly));
        Assert.Contains("cannot order", dashboard.Boundary, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("trust a price", dashboard.Boundary, StringComparison.OrdinalIgnoreCase);
    }
}
