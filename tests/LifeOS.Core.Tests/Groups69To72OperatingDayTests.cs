using LifeOS.Core.OperatingDay;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups69To72OperatingDayTests
{
    private readonly OperatingDayService _service = new();
    private readonly DateTimeOffset _now = new(2026, 7, 27, 8, 0, 0, TimeSpan.FromHours(12));

    [Fact]
    public void Proposed_calendar_blocks_never_enter_the_accepted_day_silently()
    {
        OperatingDayPlan plan = _service.Build(new[]
        {
            Item("calendar-1", "Supplier call", OperatingBoundary.Work, 9, 10, state: OperatingItemState.Proposed)
        }, new DateOnly(2026, 7, 27), _now);

        Assert.Empty(plan.Accepted);
        Assert.Single(plan.Proposed);
        Assert.True(plan.Reminders.Single().RequiresReview);
        Assert.False(plan.ClosureReady);
    }

    [Fact]
    public void Pressure_ordering_explains_why_a_reminder_is_first()
    {
        OperatingDayPlan plan = _service.Build(new[]
        {
            Item("work-1", "Protected delivery", OperatingBoundary.Work, 9, 11, pressure: 45, isProtected: true),
            Item("home-1", "Household reset", OperatingBoundary.Household, 18, 19, pressure: 20)
        }, new DateOnly(2026, 7, 27), _now);

        Assert.Equal("Protected delivery", plan.Reminders[0].Title);
        Assert.Contains("protected time", plan.Reminders[0].Reason);
    }

    [Fact]
    public void Boundary_changes_create_visible_stop_points()
    {
        OperatingDayPlan plan = _service.Build(new[]
        {
            Item("work-1", "Client delivery", OperatingBoundary.Work, 9, 11, workSessionId: "session-1"),
            Item("home-1", "Household appointment", OperatingBoundary.Household, 11, 12)
        }, new DateOnly(2026, 7, 27), _now);

        Assert.Contains(plan.StopPoints, point => point.Contains("review the boundary", StringComparison.Ordinal));
    }

    [Fact]
    public void V14_closure_requires_work_or_career_proof_links()
    {
        OperatingDayPlan withoutProof = _service.Build(new[]
        {
            Item("work-1", "Client delivery", OperatingBoundary.Work, 9, 11)
        }, new DateOnly(2026, 7, 27), _now);
        OperatingDayPlan withProof = _service.Build(new[]
        {
            Item("work-1", "Client delivery", OperatingBoundary.Work, 9, 11, workSessionId: "session-1")
        }, new DateOnly(2026, 7, 27), _now);

        Assert.False(withoutProof.ClosureReady);
        Assert.True(withProof.ClosureReady);
    }

    private OperatingDayItem Item(
        string id,
        string title,
        OperatingBoundary boundary,
        int startHour,
        int endHour,
        int pressure = 20,
        bool isProtected = false,
        string? workSessionId = null,
        OperatingItemState state = OperatingItemState.Accepted) =>
        new(
            id,
            title,
            boundary,
            _now.Date.AddHours(startHour),
            _now.Date.AddHours(endHour),
            pressure,
            isProtected,
            "fictional-local-proof",
            workSessionId,
            null,
            state);
}
