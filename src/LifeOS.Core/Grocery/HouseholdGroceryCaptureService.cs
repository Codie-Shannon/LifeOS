using System.Globalization;
using LifeOS.Core.Forms;

namespace LifeOS.Core.Grocery;

public sealed record HouseholdGroceryState(
    List<GroceryItem> Items,
    List<GroceryList> Lists,
    List<RecurringEssential> Essentials)
{
    public static HouseholdGroceryState Empty => new([], [], []);
}

public sealed record HouseholdGroceryItemDraft(
    string? ListName,
    string? ItemName,
    GroceryCategory Category,
    string? Quantity,
    string? Unit,
    GroceryPriority Priority,
    bool Required,
    bool Recurring,
    string? CadenceDays,
    string? NextDue,
    string? PreferredBrand,
    string? Notes);

public static class HouseholdGroceryCaptureService
{
    private const decimal MaximumQuantity = 100000m;

    public static FormValidationResult Validate(HouseholdGroceryItemDraft draft)
    {
        List<FormFieldIssue> issues = [];
        Add(issues, FormValidation.Required("grocery-list-name", draft.ListName, "List name"));
        Add(issues, FormValidation.MaximumLength("grocery-list-name", draft.ListName, "List name", 100));
        Add(issues, FormValidation.SingleLine("grocery-list-name", draft.ListName, "List name"));
        Add(issues, FormValidation.Required("grocery-item-name", draft.ItemName, "Item name"));
        Add(issues, FormValidation.MaximumLength("grocery-item-name", draft.ItemName, "Item name", 100));
        Add(issues, FormValidation.SingleLine("grocery-item-name", draft.ItemName, "Item name"));
        Add(issues, FormValidation.Required("grocery-quantity", draft.Quantity, "Quantity"));
        Add(issues, FormValidation.Required("grocery-unit", draft.Unit, "Unit"));
        Add(issues, FormValidation.MaximumLength("grocery-unit", draft.Unit, "Unit", 24));
        Add(issues, FormValidation.SingleLine("grocery-unit", draft.Unit, "Unit"));
        Add(issues, FormValidation.MaximumLength("grocery-brand", draft.PreferredBrand, "Preferred brand", 100));
        Add(issues, FormValidation.SingleLine("grocery-brand", draft.PreferredBrand, "Preferred brand"));
        Add(issues, FormValidation.MaximumLength("grocery-notes", draft.Notes, "Notes", 500));

        if (!string.IsNullOrWhiteSpace(draft.Quantity) &&
            (!decimal.TryParse(draft.Quantity, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal quantity) ||
             quantity <= 0 || quantity > MaximumQuantity))
        {
            issues.Add(new FormFieldIssue(
                "grocery-quantity", "quantity-range",
                "Quantity must be a number greater than 0 and no more than 100,000."));
        }

        if (draft.Recurring)
        {
            Add(issues, FormValidation.Required("grocery-cadence", draft.CadenceDays, "Cadence"));
            Add(issues, FormValidation.Required("grocery-next-due", draft.NextDue, "Next due date"));
            if (!string.IsNullOrWhiteSpace(draft.CadenceDays) &&
                (!int.TryParse(draft.CadenceDays, NumberStyles.Integer, CultureInfo.InvariantCulture, out int cadence) ||
                 cadence < 1 || cadence > 3650))
            {
                issues.Add(new FormFieldIssue(
                    "grocery-cadence", "cadence-range",
                    "Cadence must be a whole number from 1 to 3,650 days."));
            }
            if (!string.IsNullOrWhiteSpace(draft.NextDue) &&
                !DateOnly.TryParseExact(
                    draft.NextDue.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out _))
            {
                issues.Add(new FormFieldIssue(
                    "grocery-next-due", "date-format",
                    "Next due date must use YYYY-MM-DD."));
            }
        }

        return new FormValidationResult(issues);
    }

    public static HouseholdGroceryState AddItem(
        HouseholdGroceryState state,
        HouseholdGroceryItemDraft draft,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(state);
        FormValidationResult validation = Validate(draft);
        if (!validation.IsValid)
            throw new ArgumentException("The household grocery item is invalid.", nameof(draft));

        string itemId = Guid.NewGuid().ToString("N");
        string listName = draft.ListName!.Trim();
        decimal quantity = decimal.Parse(draft.Quantity!, NumberStyles.Number, CultureInfo.InvariantCulture);
        GroceryItem item = new(
            itemId,
            draft.ItemName!.Trim(),
            draft.Category,
            EmptyToNull(draft.PreferredBrand),
            [],
            EmptyToNull(draft.Notes));
        GroceryListItem listItem = new(
            Guid.NewGuid().ToString("N"),
            itemId,
            item.Name,
            new QuantityRequirement(decimal.Round(quantity, 3), draft.Unit!.Trim(), null),
            draft.Priority,
            null,
            ShoppingItemState.Pending,
            null,
            EmptyToNull(draft.Notes),
            draft.Required);

        List<GroceryList> lists = state.Lists.ToList();
        int listIndex = lists.FindIndex(list =>
            string.Equals(list.Name, listName, StringComparison.OrdinalIgnoreCase) &&
            list.State is GroceryListState.Draft or GroceryListState.Ready or
                GroceryListState.Shopping or GroceryListState.Paused);
        if (listIndex < 0)
        {
            lists.Add(new GroceryList(
                Guid.NewGuid().ToString("N"),
                listName,
                GroceryListState.Draft,
                "NZD",
                0m,
                now,
                "No estimate set",
                [listItem]));
        }
        else
        {
            GroceryList list = lists[listIndex];
            lists[listIndex] = list with { Items = [.. list.Items, listItem] };
        }

        List<RecurringEssential> essentials = state.Essentials.ToList();
        if (draft.Recurring)
        {
            essentials.Add(new RecurringEssential(
                Guid.NewGuid().ToString("N"),
                itemId,
                int.Parse(draft.CadenceDays!, CultureInfo.InvariantCulture),
                DateOnly.ParseExact(draft.NextDue!.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                listItem.Quantity,
                null,
                EssentialReviewState.Current));
        }

        return new HouseholdGroceryState([.. state.Items, item], lists, essentials);
    }

    public static HouseholdGroceryState Normalize(HouseholdGroceryState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (state.Items is null || state.Lists is null || state.Essentials is null)
            throw new InvalidDataException("The household grocery payload is incomplete.");

        List<GroceryItem> items = state.Items
            .Select(item => item with
            {
                Name = Required(item.Name, "item name", 100),
                PreferredBrand = Bounded(item.PreferredBrand, "preferred brand", 100),
                Notes = Bounded(item.Notes, "notes", 500),
                AcceptableAlternatives = item.AcceptableAlternatives?.ToArray() ?? []
            })
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        HashSet<string> itemIds = items.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);

        List<GroceryList> lists = state.Lists.Select(list =>
        {
            string name = Required(list.Name, "list name", 100);
            GroceryListItem[] listItems = (list.Items ?? [])
                .Select(entry =>
                {
                    if (!itemIds.Contains(entry.GroceryItemId) || entry.Quantity.Quantity <= 0)
                        throw new InvalidDataException("A grocery-list item is invalid.");
                    return entry with
                    {
                        RequestedName = Required(entry.RequestedName, "requested item name", 100),
                        Quantity = entry.Quantity with { Unit = Required(entry.Quantity.Unit, "unit", 24) }
                    };
                })
                .ToArray();
            return list with
            {
                Name = name,
                Currency = string.IsNullOrWhiteSpace(list.Currency) ? "NZD" : list.Currency.Trim(),
                EstimatedTotal = decimal.Round(Math.Max(0m, list.EstimatedTotal), 2),
                Items = listItems
            };
        })
            .OrderBy(list => list.State is GroceryListState.Archived or GroceryListState.Completed or GroceryListState.Cancelled)
            .ThenBy(list => list.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        List<RecurringEssential> essentials = state.Essentials.Select(essential =>
        {
            if (!itemIds.Contains(essential.GroceryItemId) || essential.CadenceDays is < 1 or > 3650)
                throw new InvalidDataException("A recurring essential is invalid.");
            return essential;
        })
            .OrderBy(essential => essential.NextDue)
            .ToList();
        return new HouseholdGroceryState(items, lists, essentials);
    }

    private static string Required(string? value, string label, int maximum)
    {
        string normalized = (value ?? string.Empty).Trim();
        if (normalized.Length is 0 || normalized.Length > maximum || normalized.Any(char.IsControl))
            throw new InvalidDataException($"The {label} is invalid.");
        return normalized;
    }

    private static string? Bounded(string? value, string label, int maximum)
    {
        string? normalized = EmptyToNull(value);
        if (normalized is not null && (normalized.Length > maximum || normalized.Any(char.IsControl)))
            throw new InvalidDataException($"The {label} is invalid.");
        return normalized;
    }

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void Add(ICollection<FormFieldIssue> issues, FormFieldIssue? issue)
    {
        if (issue is not null) issues.Add(issue);
    }
}
