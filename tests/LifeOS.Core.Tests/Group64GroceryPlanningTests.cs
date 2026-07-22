using LifeOS.Core.Grocery;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Group64GroceryPlanningTests
{
    private readonly GroceryPlanningService _service = new();

    [Fact]
    public void List_transition_rejects_invalid_jump()
    {
        var list = GroceryProofData.Build().Lists[0] with { State = GroceryListState.Draft };
        Assert.Throws<InvalidOperationException>(() => _service.Transition(list, GroceryListState.Completed));
    }

    [Fact]
    public void Overdue_and_due_soon_essentials_are_review_candidates()
    {
        var data = GroceryProofData.Build();
        var dashboard = _service.BuildDashboard(data.Lists, data.Essentials, new DateOnly(2026, 7, 22));
        Assert.Equal(2, dashboard.DueEssentials.Count);
        Assert.Contains("no automatic order", dashboard.Boundary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Template_clone_is_draft_and_estimate_only()
    {
        var source = GroceryProofData.Build().Lists[0];
        var template = new GroceryTemplate("t1", "Weekly", source.Items);
        var clone = _service.CloneTemplate(template, "new-list", DateTimeOffset.UtcNow);
        Assert.Equal(GroceryListState.Draft, clone.State);
        Assert.Equal("Manual estimate", clone.EstimateSource);
    }

    [Fact]
    public void Duplicate_candidates_remain_review_only()
    {
        var items = new[]
        {
            new GroceryItem("1","Milk",GroceryCategory.Dairy,"Brand",Array.Empty<string>(),null),
            new GroceryItem("2"," milk ",GroceryCategory.Dairy,"brand",Array.Empty<string>(),null)
        };
        Assert.True(_service.FindDuplicateCandidates(items).Single().RequiresReview);
    }
}
