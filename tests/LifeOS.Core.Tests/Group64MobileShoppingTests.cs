using LifeOS.Core.Grocery;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Group64MobileShoppingTests
{
    private readonly GroceryPlanningService _service = new();

    [Fact]
    public void Substitution_preserves_requested_item()
    {
        var list = GroceryProofData.Build().Lists[0];
        var updated = _service.ApplyAction(list, GroceryActionKind.Substitute, "li-bread", "Wholemeal loaf");
        var item = updated.Items.Single(i => i.Id == "li-bread");
        Assert.Equal("Bread", item.RequestedName);
        Assert.Equal("Wholemeal loaf", item.PurchasedName);
        Assert.Equal(ShoppingItemState.Substituted, item.State);
    }

    [Fact]
    public void Required_unresolved_item_blocks_completion()
    {
        var list = GroceryProofData.Build().Lists[0];
        Assert.False(_service.CanComplete(list));
    }

    [Fact]
    public void Offline_action_can_require_conflict_review()
    {
        var action = new OfflineGroceryAction("a1", GroceryActionKind.Check, "weekly", "li-milk", null, DateTimeOffset.UtcNow, true);
        Assert.True(action.RequiresConflictReview);
    }

    [Fact]
    public void Undo_restores_pending_state()
    {
        var list = GroceryProofData.Build().Lists[0];
        var updated = _service.ApplyAction(list, GroceryActionKind.Undo, "li-milk");
        Assert.Equal(ShoppingItemState.Pending, updated.Items.Single(i => i.Id == "li-milk").State);
    }
}
