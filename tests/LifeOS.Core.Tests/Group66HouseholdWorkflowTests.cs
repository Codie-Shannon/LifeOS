using LifeOS.Core.Grocery;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Group66HouseholdWorkflowTests
{
    private readonly HouseholdWorkflowService _service = new();

    [Fact]
    public void Overdue_and_due_soon_household_routines_surface_for_review()
    {
        var data = HouseholdWorkflowProofData.Build();

        var dashboard = _service.BuildDashboard(
            data.Routines,
            data.Assignments,
            data.Receipts,
            data.SpendReviews,
            data.ClosureChecks,
            new DateOnly(2026, 7, 22));

        Assert.Contains(dashboard.DueRoutines, routine => routine.Title == "Fridge and freezer check" && routine.State == HouseholdRoutineState.Overdue);
        Assert.Contains(dashboard.DueRoutines, routine => routine.Title == "Bin night reset" && routine.State == HouseholdRoutineState.DueSoon);
    }

    [Fact]
    public void Proposed_assignments_require_explicit_acceptance()
    {
        var data = HouseholdWorkflowProofData.Build();

        var dashboard = _service.BuildDashboard(
            data.Routines,
            data.Assignments,
            data.Receipts,
            data.SpendReviews,
            data.ClosureChecks,
            new DateOnly(2026, 7, 22));

        var proposed = dashboard.Assignments.Single(assignment => assignment.Id == "assign-fridge");
        var accepted = _service.AcceptAssignment(proposed);

        Assert.True(proposed.RequiresReview);
        Assert.Equal(HouseholdAssignmentState.Accepted, accepted.State);
        Assert.False(accepted.RequiresReview);
    }

    [Fact]
    public void Receipt_linkage_stays_a_review_candidate()
    {
        var data = HouseholdWorkflowProofData.Build();
        var receipt = data.Receipts.Single(candidate => candidate.Id == "receipt-market");

        var linked = _service.MarkReceiptLinkedToDocumentCandidate(receipt);

        Assert.Equal(HouseholdReceiptState.LinkedToDocumentCandidate, linked.State);
        Assert.True(linked.RequiresReview);
        Assert.Contains("Documents", linked.SuggestedDocumentTarget, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Money", linked.SuggestedMoneyTarget, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Planned_vs_actual_spend_overage_requires_review()
    {
        var data = HouseholdWorkflowProofData.Build();

        var dashboard = _service.BuildDashboard(
            data.Routines,
            data.Assignments,
            data.Receipts,
            data.SpendReviews,
            data.ClosureChecks,
            new DateOnly(2026, 7, 22));

        var groceries = dashboard.SpendReviews.Single(review => review.Category == "Groceries");

        Assert.Equal(HouseholdSpendState.OverPlan, groceries.State);
        Assert.True(groceries.RequiresReview);
    }

    [Fact]
    public void V13_closure_blocks_until_all_closure_checks_pass()
    {
        var data = HouseholdWorkflowProofData.Build();

        var dashboard = _service.BuildDashboard(
            data.Routines,
            data.Assignments,
            data.Receipts,
            data.SpendReviews,
            data.ClosureChecks,
            new DateOnly(2026, 7, 22));

        Assert.Equal(V13ClosureState.Blocked, dashboard.ClosureState);
        Assert.Contains("cannot mutate Money", dashboard.Boundary, StringComparison.OrdinalIgnoreCase);
    }
}
