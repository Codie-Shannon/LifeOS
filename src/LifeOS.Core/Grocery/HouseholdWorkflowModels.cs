namespace LifeOS.Core.Grocery;

public enum HouseholdRoutineState { Pending, DueSoon, Overdue, Completed, Deferred }
public enum HouseholdAssignmentState { Proposed, Accepted, Deferred, Declined, Completed }
public enum HouseholdReceiptState { Captured, NeedsReview, LinkedToDocumentCandidate, LinkedToMoneyCandidate, Rejected }
public enum HouseholdSpendState { OnTrack, OverPlan, UnderPlan, NeedsReview }
public enum V13ClosureState { Ready, Blocked }

public sealed record HouseholdRoutine(
    string Id,
    string Title,
    string Area,
    int CadenceDays,
    DateOnly NextDue,
    HouseholdRoutineState State,
    string? AssignedTo,
    string SourceEvidence);

public sealed record HouseholdAssignment(
    string Id,
    string RoutineId,
    string AssignedTo,
    HouseholdAssignmentState State,
    DateOnly ProposedFor,
    bool RequiresReview,
    string SourceEvidence);

public sealed record HouseholdReceiptCandidate(
    string Id,
    string Title,
    decimal Amount,
    string Currency,
    DateOnly CapturedOn,
    HouseholdReceiptState State,
    string SuggestedDocumentTarget,
    string SuggestedMoneyTarget,
    bool RequiresReview,
    string SourceEvidence);

public sealed record HouseholdSpendReview(
    string Id,
    string Category,
    decimal PlannedAmount,
    decimal ActualAmount,
    string Currency,
    HouseholdSpendState State,
    bool RequiresReview,
    string SourceEvidence);

public sealed record V13ClosureCheck(
    string Id,
    string Title,
    bool Passed,
    string Evidence);

public sealed record HouseholdWorkflowDashboard(
    IReadOnlyList<HouseholdRoutine> DueRoutines,
    IReadOnlyList<HouseholdAssignment> Assignments,
    IReadOnlyList<HouseholdReceiptCandidate> ReceiptCandidates,
    IReadOnlyList<HouseholdSpendReview> SpendReviews,
    IReadOnlyList<V13ClosureCheck> ClosureChecks,
    V13ClosureState ClosureState,
    string Boundary);

public sealed class HouseholdWorkflowService
{
    public HouseholdWorkflowDashboard BuildDashboard(
        IReadOnlyList<HouseholdRoutine> routines,
        IReadOnlyList<HouseholdAssignment> assignments,
        IReadOnlyList<HouseholdReceiptCandidate> receipts,
        IReadOnlyList<HouseholdSpendReview> spendReviews,
        IReadOnlyList<V13ClosureCheck> closureChecks,
        DateOnly today)
    {
        var dueRoutines = routines
            .Select(routine => CalculateRoutineState(routine, today))
            .Where(routine => routine.State is HouseholdRoutineState.Overdue or HouseholdRoutineState.DueSoon or HouseholdRoutineState.Pending)
            .ToArray();

        var reviewedAssignments = assignments
            .Select(assignment => assignment with { RequiresReview = assignment.State == HouseholdAssignmentState.Proposed || assignment.RequiresReview })
            .ToArray();

        var reviewedReceipts = receipts
            .Select(receipt => receipt with { RequiresReview = receipt.State is HouseholdReceiptState.Captured or HouseholdReceiptState.NeedsReview || receipt.RequiresReview })
            .ToArray();

        var reviewedSpend = spendReviews
            .Select(review =>
            {
                var state = review.ActualAmount > review.PlannedAmount
                    ? HouseholdSpendState.OverPlan
                    : review.State;

                return review with
                {
                    State = state,
                    RequiresReview = state is HouseholdSpendState.OverPlan or HouseholdSpendState.NeedsReview || review.RequiresReview
                };
            })
            .ToArray();

        var closureState = closureChecks.All(check => check.Passed)
            ? V13ClosureState.Ready
            : V13ClosureState.Blocked;

        return new HouseholdWorkflowDashboard(
            dueRoutines,
            reviewedAssignments,
            reviewedReceipts,
            reviewedSpend,
            closureChecks,
            closureState,
            "Review-first only: household routines, assignments, receipt links and spend reviews cannot mutate Money, Documents, grocery lists, carts or payments automatically.");
    }

    public HouseholdRoutine DeferRoutine(HouseholdRoutine routine, DateOnly nextDue) =>
        routine with { NextDue = nextDue, State = HouseholdRoutineState.Deferred };

    public HouseholdAssignment AcceptAssignment(HouseholdAssignment assignment) =>
        assignment with { State = HouseholdAssignmentState.Accepted, RequiresReview = false };

    public HouseholdReceiptCandidate MarkReceiptLinkedToDocumentCandidate(HouseholdReceiptCandidate receipt) =>
        receipt with { State = HouseholdReceiptState.LinkedToDocumentCandidate, RequiresReview = true };

    private static HouseholdRoutine CalculateRoutineState(HouseholdRoutine routine, DateOnly today)
    {
        if (routine.State is HouseholdRoutineState.Completed or HouseholdRoutineState.Deferred)
        {
            return routine;
        }

        if (routine.NextDue < today)
        {
            return routine with { State = HouseholdRoutineState.Overdue };
        }

        if (routine.NextDue <= today.AddDays(3))
        {
            return routine with { State = HouseholdRoutineState.DueSoon };
        }

        return routine with { State = HouseholdRoutineState.Pending };
    }
}

public static class HouseholdWorkflowProofData
{
    public static (
        IReadOnlyList<HouseholdRoutine> Routines,
        IReadOnlyList<HouseholdAssignment> Assignments,
        IReadOnlyList<HouseholdReceiptCandidate> Receipts,
        IReadOnlyList<HouseholdSpendReview> SpendReviews,
        IReadOnlyList<V13ClosureCheck> ClosureChecks) Build()
    {
        var routines = new[]
        {
            new HouseholdRoutine("fridge-check", "Fridge and freezer check", "Kitchen", 7, new DateOnly(2026, 7, 21),
                HouseholdRoutineState.Pending, "CS", "Synthetic household routine"),
            new HouseholdRoutine("bin-night", "Bin night reset", "Household", 7, new DateOnly(2026, 7, 24),
                HouseholdRoutineState.Pending, null, "Synthetic household routine"),
            new HouseholdRoutine("pantry-review", "Pantry stock review", "Kitchen", 14, new DateOnly(2026, 7, 30),
                HouseholdRoutineState.Pending, null, "Synthetic household routine")
        };

        var assignments = new[]
        {
            new HouseholdAssignment("assign-fridge", "fridge-check", "CS", HouseholdAssignmentState.Proposed,
                new DateOnly(2026, 7, 22), true, "Synthetic assignment proposal"),
            new HouseholdAssignment("assign-bin", "bin-night", "CS", HouseholdAssignmentState.Accepted,
                new DateOnly(2026, 7, 24), false, "Synthetic assignment proposal")
        };

        var receipts = new[]
        {
            new HouseholdReceiptCandidate("receipt-market", "Local supermarket receipt", 74.30m, "NZD",
                new DateOnly(2026, 7, 22), HouseholdReceiptState.NeedsReview,
                "Documents: receipt evidence candidate", "Money: household grocery spend candidate", true,
                "Synthetic receipt capture"),
            new HouseholdReceiptCandidate("receipt-pharmacy", "Local pharmacy receipt", 18.90m, "NZD",
                new DateOnly(2026, 7, 21), HouseholdReceiptState.Captured,
                "Documents: receipt evidence candidate", "Money: household health spend candidate", true,
                "Synthetic receipt capture")
        };

        var spend = new[]
        {
            new HouseholdSpendReview("grocery-week", "Groceries", 90m, 93.20m, "NZD",
                HouseholdSpendState.NeedsReview, true, "Synthetic planned vs actual review"),
            new HouseholdSpendReview("pharmacy-week", "Pharmacy", 25m, 18.90m, "NZD",
                HouseholdSpendState.OnTrack, false, "Synthetic planned vs actual review")
        };

        var checks = new[]
        {
            new V13ClosureCheck("desktop", "Desktop household workflow surfaces", true, "Synthetic Desktop proof ready"),
            new V13ClosureCheck("mobile", "Full Mobile household workflow surfaces", true, "Synthetic Mobile proof ready"),
            new V13ClosureCheck("tests", "Core household workflow regression tests", true, "Automated tests required"),
            new V13ClosureCheck("screenshots", "Group 66 screenshot pack", false, "Pending capture")
        };

        return (routines, assignments, receipts, spend, checks);
    }
}
