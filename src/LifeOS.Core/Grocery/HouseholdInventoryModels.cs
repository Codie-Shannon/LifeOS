namespace LifeOS.Core.Grocery;

public enum HouseholdStockState { Unknown, Out, Low, Enough, Overstocked }
public enum HouseholdInventoryCandidateKind { StockReview, ExpiryReview, RecipeIngredientGap, StorePriceContext }
public enum StoreProfileKind { Supermarket, Warehouse, Pharmacy, LocalShop, Online, Other }

public sealed record HouseholdInventoryItem(
    string Id,
    string Name,
    GroceryCategory Category,
    HouseholdStockState StockState,
    decimal OnHand,
    string Unit,
    string? StorageLocation,
    DateOnly? LastChecked,
    DateOnly? ExpiresOn,
    string SourceEvidence,
    string? Notes);

public sealed record StoreProfile(
    string Id,
    string Name,
    StoreProfileKind Kind,
    string? Area,
    IReadOnlyList<GroceryCategory> StrongCategories,
    bool PriceContextOnly,
    string SourceEvidence);

public sealed record RecipeIngredient(
    string GroceryItemId,
    string Name,
    QuantityRequirement Quantity,
    bool Required);

public sealed record MealRecipe(
    string Id,
    string Name,
    IReadOnlyList<RecipeIngredient> Ingredients,
    string SourceEvidence);

public sealed record HouseholdInventoryCandidate(
    HouseholdInventoryCandidateKind Kind,
    string Title,
    string SuggestedTarget,
    string SuggestedAction,
    bool RequiresReview,
    string SourceEvidence);

public sealed record HouseholdInventoryDashboard(
    int OutCount,
    int LowCount,
    int EnoughCount,
    int OverstockedCount,
    int UnknownCount,
    IReadOnlyList<HouseholdInventoryCandidate> ReviewCandidates,
    IReadOnlyList<StoreProfile> StoreProfiles,
    IReadOnlyList<MealRecipe> Recipes,
    string Boundary);

public sealed class HouseholdInventoryService
{
    public HouseholdInventoryDashboard BuildDashboard(
        IReadOnlyList<HouseholdInventoryItem> inventory,
        IReadOnlyList<StoreProfile> stores,
        IReadOnlyList<MealRecipe> recipes,
        DateOnly today)
    {
        var candidates = new List<HouseholdInventoryCandidate>();

        foreach (HouseholdInventoryItem item in inventory)
        {
            if (item.StockState is HouseholdStockState.Out or HouseholdStockState.Low or HouseholdStockState.Unknown)
            {
                candidates.Add(
                    new HouseholdInventoryCandidate(
                        HouseholdInventoryCandidateKind.StockReview,
                        item.Name,
                        "Inventory",
                        item.StockState == HouseholdStockState.Unknown
                            ? "Check stock before adding to any grocery list."
                            : "Review whether this should become a grocery-list candidate.",
                        true,
                        item.SourceEvidence));
            }

            if (item.ExpiresOn is { } expiresOn && expiresOn <= today.AddDays(3))
            {
                candidates.Add(
                    new HouseholdInventoryCandidate(
                        HouseholdInventoryCandidateKind.ExpiryReview,
                        item.Name,
                        "Inventory",
                        $"Check expiry by {expiresOn:dd MMM yyyy} before relying on this stock.",
                        true,
                        item.SourceEvidence));
            }
        }

        foreach (MealRecipe recipe in recipes)
        {
            foreach (RecipeIngredient ingredient in recipe.Ingredients.Where(ingredient => ingredient.Required))
            {
                HouseholdInventoryItem? matched = inventory.FirstOrDefault(
                    item => string.Equals(item.Id, ingredient.GroceryItemId, StringComparison.OrdinalIgnoreCase));

                if (matched is null || matched.StockState is HouseholdStockState.Out or HouseholdStockState.Low or HouseholdStockState.Unknown)
                {
                    candidates.Add(
                        new HouseholdInventoryCandidate(
                            HouseholdInventoryCandidateKind.RecipeIngredientGap,
                            $"{recipe.Name}: {ingredient.Name}",
                            "MealIngredients",
                            "Review ingredient gap before cloning anything into a grocery list.",
                            true,
                            recipe.SourceEvidence));
                }
            }
        }

        foreach (StoreProfile store in stores.Where(store => store.PriceContextOnly))
        {
            candidates.Add(
                new HouseholdInventoryCandidate(
                    HouseholdInventoryCandidateKind.StorePriceContext,
                    store.Name,
                    "StoreProfiles",
                    "Use store profile as planning context only. It is not a trusted price feed.",
                    true,
                    store.SourceEvidence));
        }

        return new HouseholdInventoryDashboard(
            inventory.Count(item => item.StockState == HouseholdStockState.Out),
            inventory.Count(item => item.StockState == HouseholdStockState.Low),
            inventory.Count(item => item.StockState == HouseholdStockState.Enough),
            inventory.Count(item => item.StockState == HouseholdStockState.Overstocked),
            inventory.Count(item => item.StockState == HouseholdStockState.Unknown),
            candidates,
            stores,
            recipes,
            "Review-first only: inventory, meals, stores and price context cannot order, pay, trust a price or mutate grocery lists automatically.");
    }

    public IReadOnlyList<HouseholdInventoryCandidate> BuildGroceryReviewCandidates(
        HouseholdInventoryDashboard dashboard) =>
        dashboard.ReviewCandidates
            .Where(candidate => candidate.Kind is HouseholdInventoryCandidateKind.StockReview or HouseholdInventoryCandidateKind.RecipeIngredientGap)
            .ToArray();
}

public static class HouseholdInventoryProofData
{
    public static (
        IReadOnlyList<HouseholdInventoryItem> Inventory,
        IReadOnlyList<StoreProfile> Stores,
        IReadOnlyList<MealRecipe> Recipes) Build()
    {
        var inventory = new[]
        {
            new HouseholdInventoryItem("milk", "Milk", GroceryCategory.Dairy, HouseholdStockState.Low, 0.5m, "L", "Fridge",
                new DateOnly(2026, 7, 22), new DateOnly(2026, 7, 23), "Synthetic household stocktake", "Used for breakfast and coffee."),
            new HouseholdInventoryItem("bread", "Bread", GroceryCategory.Bakery, HouseholdStockState.Out, 0m, "loaf", "Pantry",
                new DateOnly(2026, 7, 22), null, "Synthetic household stocktake", "Can substitute wraps."),
            new HouseholdInventoryItem("rice", "Rice", GroceryCategory.Pantry, HouseholdStockState.Overstocked, 3m, "kg", "Pantry",
                new DateOnly(2026, 7, 20), null, "Synthetic pantry count", "Do not buy more."),
            new HouseholdInventoryItem("bananas", "Bananas", GroceryCategory.Produce, HouseholdStockState.Enough, 6m, "each", "Bench",
                new DateOnly(2026, 7, 22), new DateOnly(2026, 7, 25), "Synthetic household stocktake", null),
            new HouseholdInventoryItem("dish-tabs", "Dishwasher tablets", GroceryCategory.Cleaning, HouseholdStockState.Unknown, 0m, "tabs", "Kitchen cupboard",
                null, null, "Synthetic cupboard review", "Check box before shopping.")
        };

        var stores = new[]
        {
            new StoreProfile("local-market", "Local supermarket", StoreProfileKind.Supermarket, "Whakatane",
                new[] { GroceryCategory.Produce, GroceryCategory.Dairy, GroceryCategory.Bakery }, true, "Synthetic store profile"),
            new StoreProfile("chemist", "Local pharmacy", StoreProfileKind.Pharmacy, "Whakatane",
                new[] { GroceryCategory.PersonalCare }, true, "Synthetic store profile")
        };

        var recipes = new[]
        {
            new MealRecipe(
                "breakfast",
                "Simple breakfast",
                new[]
                {
                    new RecipeIngredient("milk", "Milk", new QuantityRequirement(0.25m, "L", null), true),
                    new RecipeIngredient("bread", "Bread", new QuantityRequirement(2m, "slices", null), true),
                    new RecipeIngredient("bananas", "Bananas", new QuantityRequirement(1m, "each", null), false)
                },
                "Synthetic meal plan"),
            new MealRecipe(
                "rice-bowl",
                "Rice bowl",
                new[]
                {
                    new RecipeIngredient("rice", "Rice", new QuantityRequirement(0.2m, "kg", null), true),
                    new RecipeIngredient("sauce", "Sauce", new QuantityRequirement(1m, "bottle", null), true)
                },
                "Synthetic meal plan")
        };

        return (inventory, stores, recipes);
    }
}
