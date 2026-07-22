namespace LifeOS.Core.Grocery;

public sealed class GroceryPlanningService
{
    public GroceryList Transition(GroceryList list, GroceryListState next)
    {
        var valid = list.State switch
        {
            GroceryListState.Draft => next is GroceryListState.Ready or GroceryListState.Cancelled,
            GroceryListState.Ready => next is GroceryListState.Shopping or GroceryListState.Cancelled,
            GroceryListState.Shopping => next is GroceryListState.Paused or GroceryListState.Completed,
            GroceryListState.Paused => next is GroceryListState.Shopping or GroceryListState.Cancelled,
            GroceryListState.Completed => next is GroceryListState.Archived,
            GroceryListState.Cancelled => next is GroceryListState.Archived,
            _ => false
        };

        if (!valid) throw new InvalidOperationException($"Invalid grocery-list transition: {list.State} -> {next}.");
        return list with { State = next };
    }

    public EssentialReviewState CalculateEssentialState(RecurringEssential essential, DateOnly today)
    {
        if (essential.ReviewState is EssentialReviewState.Deferred or EssentialReviewState.SkippedOnce or EssentialReviewState.Accepted)
            return essential.ReviewState;
        if (essential.NextDue < today) return EssentialReviewState.Due;
        if (essential.NextDue <= today.AddDays(3)) return EssentialReviewState.DueSoon;
        return EssentialReviewState.Current;
    }

    public RecurringEssential Defer(RecurringEssential essential, DateOnly newDate) =>
        essential with { NextDue = newDate, ReviewState = EssentialReviewState.Deferred };

    public RecurringEssential SkipOnce(RecurringEssential essential) =>
        essential with
        {
            NextDue = essential.NextDue.AddDays(essential.CadenceDays),
            ReviewState = EssentialReviewState.SkippedOnce
        };

    public GroceryList CloneTemplate(GroceryTemplate template, string listId, DateTimeOffset now) =>
        new(listId, template.Name, GroceryListState.Draft, "NZD", 0m, now, "Manual estimate", template.Items);

    public IReadOnlyList<DuplicateGroceryCandidate> FindDuplicateCandidates(
        IReadOnlyList<GroceryItem> items)
    {
        static string Normalize(GroceryItem item) =>
            string.Join('|', item.Name.Trim().ToUpperInvariant(), item.PreferredBrand?.Trim().ToUpperInvariant() ?? "");

        return items.GroupBy(Normalize)
            .Where(group => group.Count() > 1)
            .SelectMany(group =>
            {
                var first = group.First();
                return group.Skip(1).Select(item =>
                    new DuplicateGroceryCandidate(first.Id, item.Id, "Normalized name and brand match", true));
            })
            .ToArray();
    }

    public GroceryList ApplyAction(GroceryList list, GroceryActionKind action, string itemId, string? value = null)
    {
        var updated = list.Items.Select(item =>
        {
            if (!string.Equals(item.Id, itemId, StringComparison.Ordinal)) return item;
            return action switch
            {
                GroceryActionKind.Check => item with { State = ShoppingItemState.Checked },
                GroceryActionKind.Undo => item with { State = ShoppingItemState.Pending, PurchasedName = null },
                GroceryActionKind.MarkUnavailable => item with { State = ShoppingItemState.Unavailable },
                GroceryActionKind.Substitute when !string.IsNullOrWhiteSpace(value) =>
                    item with { State = ShoppingItemState.Substituted, PurchasedName = value },
                GroceryActionKind.Skip => item with { State = ShoppingItemState.Skipped },
                GroceryActionKind.AddNote => item with { Note = value },
                GroceryActionKind.AdjustQuantity when decimal.TryParse(value, out var quantity) && quantity > 0 =>
                    item with { Quantity = item.Quantity with { Quantity = quantity } },
                _ => item
            };
        }).ToArray();

        return list with { Items = updated };
    }

    public bool CanComplete(GroceryList list) =>
        list.Items.All(item => !item.Required || item.State is ShoppingItemState.Checked or ShoppingItemState.Substituted or ShoppingItemState.Skipped);

    public GroceryDashboard BuildDashboard(
        IReadOnlyList<GroceryList> lists,
        IReadOnlyList<RecurringEssential> essentials,
        DateOnly today)
    {
        var active = lists.FirstOrDefault(list => list.State is GroceryListState.Ready or GroceryListState.Shopping or GroceryListState.Paused);
        var due = essentials.Where(item => CalculateEssentialState(item, today) is EssentialReviewState.Due or EssentialReviewState.DueSoon).ToArray();
        var completed = lists.Where(list => list.State == GroceryListState.Completed).Take(3).ToArray();
        var unresolved = active?.Items.Count(item => item.Required && item.State == ShoppingItemState.Pending) ?? 0;
        return new GroceryDashboard(active, due, completed, unresolved,
            "Review-first only: no automatic order, payment, external-cart mutation or trusted transaction.");
    }
}

public static class GroceryProofData
{
    public static (IReadOnlyList<GroceryItem> Items, IReadOnlyList<GroceryList> Lists, IReadOnlyList<RecurringEssential> Essentials) Build()
    {
        var items = new[]
        {
            new GroceryItem("milk", "Milk", GroceryCategory.Dairy, "Dairyworks", new[] { "Store brand" }, "2L bottle"),
            new GroceryItem("bread", "Bread", GroceryCategory.Bakery, null, new[] { "Wholemeal loaf" }, null),
            new GroceryItem("bananas", "Bananas", GroceryCategory.Produce, null, Array.Empty<string>(), "Ripe but firm"),
            new GroceryItem("cleaner", "Surface cleaner", GroceryCategory.Cleaning, null, new[] { "Multi-purpose spray" }, null)
        };
        var listItems = new[]
        {
            new GroceryListItem("li-milk", "milk", "Milk", new(2, "L", "1 x 2L"), GroceryPriority.Essential, new DateOnly(2026,7,22), ShoppingItemState.Checked, null, null, true),
            new GroceryListItem("li-bread", "bread", "Bread", new(1, "loaf", null), GroceryPriority.High, new DateOnly(2026,7,22), ShoppingItemState.Substituted, "Wholemeal loaf", "User-approved substitute", true),
            new GroceryListItem("li-bananas", "bananas", "Bananas", new(6, "each", null), GroceryPriority.Normal, null, ShoppingItemState.Pending, null, null, false),
            new GroceryListItem("li-cleaner", "cleaner", "Surface cleaner", new(1, "bottle", null), GroceryPriority.High, null, ShoppingItemState.Unavailable, null, "Check alternate store", true)
        };
        var lists = new[]
        {
            new GroceryList("weekly", "Weekly household shop", GroceryListState.Shopping, "NZD", 68.40m,
                new DateTimeOffset(2026,7,22,16,0,0,TimeSpan.FromHours(12)), "Manual estimate â€¢ fresh 22 Jul 16:00", listItems),
            new GroceryList("completed", "Top-up shop", GroceryListState.Completed, "NZD", 21.80m,
                new DateTimeOffset(2026,7,19,12,0,0,TimeSpan.FromHours(12)), "Manual estimate", Array.Empty<GroceryListItem>())
        };
        var essentials = new[]
        {
            new RecurringEssential("e-milk", "milk", 7, new DateOnly(2026,7,21), new(2,"L","1 x 2L"), "Local supermarket", EssentialReviewState.Due),
            new RecurringEssential("e-bread", "bread", 7, new DateOnly(2026,7,24), new(1,"loaf",null), "Local supermarket", EssentialReviewState.DueSoon),
            new RecurringEssential("e-cleaner", "cleaner", 30, new DateOnly(2026,8,3), new(1,"bottle",null), null, EssentialReviewState.Deferred),
            new RecurringEssential("e-bananas", "bananas", 7, new DateOnly(2026,7,29), new(6,"each",null), null, EssentialReviewState.SkippedOnce)
        };
        return (items, lists, essentials);
    }
}
