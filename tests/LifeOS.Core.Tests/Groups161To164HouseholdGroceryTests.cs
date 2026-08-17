using System.Text.Json;
using LifeOS.Core.Grocery;
using LifeOS.Shared.Grocery;
using LifeOS.Shared.Storage;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups161To164HouseholdGroceryTests
{
    [Fact]
    public void Capture_requires_list_item_quantity_and_unit_with_bounded_quantity()
    {
        var missing = HouseholdGroceryCaptureService.Validate(Draft() with
        {
            ListName = null, ItemName = null, Quantity = null, Unit = null
        });
        var range = HouseholdGroceryCaptureService.Validate(Draft() with { Quantity = "100001" });

        Assert.False(missing.IsValid);
        Assert.Contains(missing.ForField("grocery-list-name"), issue => issue.Code == "required");
        Assert.Contains(missing.ForField("grocery-item-name"), issue => issue.Code == "required");
        Assert.Equal("quantity-range", Assert.Single(range.ForField("grocery-quantity")).Code);
    }

    [Fact]
    public void Recurring_capture_requires_valid_cadence_and_iso_date()
    {
        var result = HouseholdGroceryCaptureService.Validate(Draft() with
        {
            Recurring = true,
            CadenceDays = "0",
            NextDue = "18/08/2026"
        });

        Assert.False(result.IsValid);
        Assert.Equal("cadence-range", Assert.Single(result.ForField("grocery-cadence")).Code);
        Assert.Equal("date-format", Assert.Single(result.ForField("grocery-next-due")).Code);
    }

    [Fact]
    public void Add_creates_local_draft_list_item_and_recurring_essential()
    {
        DateTimeOffset now = new(2026, 8, 18, 1, 2, 3, TimeSpan.Zero);
        HouseholdGroceryState state = HouseholdGroceryCaptureService.AddItem(
            HouseholdGroceryState.Empty,
            Draft() with { Recurring = true, CadenceDays = "7", NextDue = "2026-08-25" },
            now);

        GroceryItem item = Assert.Single(state.Items);
        GroceryList list = Assert.Single(state.Lists);
        GroceryListItem listItem = Assert.Single(list.Items);
        RecurringEssential essential = Assert.Single(state.Essentials);
        Assert.Equal(GroceryListState.Draft, list.State);
        Assert.Equal(item.Id, listItem.GroceryItemId);
        Assert.Equal(item.Id, essential.GroceryItemId);
        Assert.Equal(new DateOnly(2026, 8, 25), essential.NextDue);
        Assert.Equal("No estimate set", list.EstimateSource);
    }

    [Fact]
    public void Repeated_name_appends_distinct_items_and_duplicate_is_review_candidate_only()
    {
        HouseholdGroceryState first = HouseholdGroceryCaptureService.AddItem(
            HouseholdGroceryState.Empty, Draft(), DateTimeOffset.UtcNow);
        HouseholdGroceryState second = HouseholdGroceryCaptureService.AddItem(
            first, Draft(), DateTimeOffset.UtcNow);

        Assert.Equal(2, second.Items.Count);
        Assert.Equal(2, Assert.Single(second.Lists).Items.Count);
        Assert.NotEqual(second.Items[0].Id, second.Items[1].Id);
        Assert.True(Assert.Single(new GroceryPlanningService().FindDuplicateCandidates(second.Items)).RequiresReview);
    }

    [Fact]
    public void Shopping_transitions_and_required_completion_remain_explicit()
    {
        GroceryPlanningService service = new();
        HouseholdGroceryState state = HouseholdGroceryCaptureService.AddItem(
            HouseholdGroceryState.Empty, Draft(), DateTimeOffset.UtcNow);
        GroceryList draft = Assert.Single(state.Lists);
        GroceryList ready = service.Transition(draft, GroceryListState.Ready);
        GroceryList shopping = service.Transition(ready, GroceryListState.Shopping);

        Assert.False(service.CanComplete(shopping));
        Assert.Throws<InvalidOperationException>(() => service.Transition(draft, GroceryListState.Completed));

        GroceryList checkedList = service.ApplyAction(
            shopping, GroceryActionKind.Check, Assert.Single(shopping.Items).Id);
        Assert.True(service.CanComplete(checkedList));
        Assert.Equal(GroceryListState.Completed, service.Transition(checkedList, GroceryListState.Completed).State);
    }

    [Fact]
    public void Missing_repository_returns_honest_empty_state_without_writing()
    {
        using TemporaryDirectory temporary = new();
        string path = Path.Combine(temporary.Path, "household-grocery.json");
        HouseholdGroceryRepository repository = new(path);

        LocalStoreLoadResult<HouseholdGroceryState> result = repository.LoadResult();

        Assert.Equal(LocalStoreLoadState.Empty, result.State);
        Assert.Empty(result.Value.Items);
        Assert.Empty(result.Value.Lists);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void Repository_round_trips_versioned_normalized_state()
    {
        using TemporaryDirectory temporary = new();
        string path = Path.Combine(temporary.Path, "household-grocery.json");
        HouseholdGroceryRepository repository = new(path);
        HouseholdGroceryState state = HouseholdGroceryCaptureService.AddItem(
            HouseholdGroceryState.Empty,
            Draft() with { ItemName = "  Milk  ", Quantity = "2.5000", Unit = " L " },
            DateTimeOffset.UtcNow);
        repository.Save(state);

        HouseholdGroceryState loaded = repository.Load();
        using JsonDocument json = JsonDocument.Parse(File.ReadAllText(path));

        Assert.Equal("Milk", Assert.Single(loaded.Items).Name);
        Assert.Equal(2.5m, Assert.Single(Assert.Single(loaded.Lists).Items).Quantity.Quantity);
        Assert.Equal("household-grocery", json.RootElement.GetProperty("storeId").GetString());
        Assert.Equal(1, json.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(LocalStoreHealthState.Healthy, repository.Inspect().State);
    }

    [Fact]
    public void Invalid_reference_is_rejected_and_trash_restore_is_recoverable()
    {
        using TemporaryDirectory temporary = new();
        string path = Path.Combine(temporary.Path, "household-grocery.json");
        HouseholdGroceryRepository repository = new(path);
        HouseholdGroceryState valid = HouseholdGroceryCaptureService.AddItem(
            HouseholdGroceryState.Empty, Draft(), DateTimeOffset.UtcNow);
        GroceryList list = Assert.Single(valid.Lists);
        HouseholdGroceryState invalid = valid with
        {
            Lists = [list with { Items = [Assert.Single(list.Items) with { GroceryItemId = "missing" }] }]
        };

        Assert.Throws<InvalidDataException>(() => repository.Save(invalid));
        repository.Save(valid);
        LocalStoreTrashEntry trash = repository.MoveToTrash();
        repository.RestoreTrash(trash.Id);

        Assert.Single(repository.Load().Items);
        Assert.Throws<InvalidOperationException>(() => repository.RestoreTrash(trash.Id));
    }

    private static HouseholdGroceryItemDraft Draft() => new(
        "Weekly shop",
        "Milk",
        GroceryCategory.Dairy,
        "2",
        "L",
        GroceryPriority.Essential,
        true,
        false,
        null,
        null,
        "Store brand",
        "Breakfast and coffee");

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "lifeos-household-grocery-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true);
        }
    }
}
