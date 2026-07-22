using LifeOS.Core.CareerStudio;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Group61CareerApplicationTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 22, 14, 0, 0, TimeSpan.FromHours(12));

    [Fact]
    public void ApplicationCreationRequiresExplicitApprovedOpportunityAction()
    {
        var opportunity = CareerProofData.Build(Now).Opportunities[0];
        var service = new CareerApplicationService();
        Assert.False(service.CreateFromApprovedOpportunity(opportunity, false, Now).Applied);
        Assert.True(service.CreateFromApprovedOpportunity(opportunity, true, Now).Applied);
    }

    [Fact]
    public void ChecklistControlsReadinessAndCanReopen()
    {
        var proof = CareerProofData.Build(Now);
        var service = new CareerApplicationService();
        var app = service.CreateFromApprovedOpportunity(proof.Opportunities[0], true, Now).Application;
        foreach (var item in app.Checklist.Where(x => x.IsRequired))
            app = service.SetChecklistItem(app, item.Id, true, Now.AddMinutes(1)).Application;
        Assert.Equal(ApplicationState.ReadyToSubmit, app.State);
        var firstRequired = app.Checklist.First(x => x.IsRequired);
        app = service.SetChecklistItem(app, firstRequired.Id, false, Now.AddMinutes(2)).Application;
        Assert.Equal(ApplicationState.Preparing, app.State);
    }

    [Fact]
    public void SubmissionIsRecordedButNeverPerformedByLifeOs()
    {
        var app = CareerApplicationProofData.Build(CareerProofData.Build(Now), Now).Single();
        Assert.Equal("External job board", app.SubmissionChannel);
        Assert.Contains(app.Timeline, x => x.SafeSummary.Contains("did not send", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DashboardCalculatesClosingSoonOverdueAndWaitingOn()
    {
        var proof = CareerProofData.Build(Now);
        var apps = CareerApplicationProofData.Build(proof, Now);
        var summary = new CareerApplicationService().BuildDashboard(proof.Opportunities, apps, Now);
        Assert.True(summary.ClosingSoon > 0);
        Assert.True(summary.OverdueFollowUps > 0);
        Assert.True(summary.WaitingOn > 0);
    }

    [Fact]
    public void InterviewAndCalendarContextRemainReadOnlyRecords()
    {
        var interview = CareerApplicationProofData.Build(CareerProofData.Build(Now), Now).Single().Interviews.Single();
        Assert.NotNull(interview.CalendarContextId);
        Assert.False(interview.PreparationComplete);
    }
}
