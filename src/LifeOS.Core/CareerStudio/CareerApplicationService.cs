namespace LifeOS.Core.CareerStudio;

public sealed class CareerApplicationService
{
    public ApplicationActionResult CreateFromApprovedOpportunity(
        CareerOpportunity opportunity,
        bool explicitlyApproved,
        DateTimeOffset now)
    {
        if (!explicitlyApproved)
            return new(false, null!, "Explicit approval is required.");
        if (opportunity.Stage is OpportunityStage.Discovered or OpportunityStage.Archived or OpportunityStage.Declined or OpportunityStage.Rejected)
            return new(false, null!, "Opportunity is not approved for application preparation.");

        var checklist = Enum.GetValues<ChecklistItemType>()
            .Select(type => new ApplicationChecklistItem(
                $"check-{opportunity.Id}-{type}",
                type,
                type switch
                {
                    ChecklistItemType.Cv => "Role-specific CV",
                    ChecklistItemType.CoverLetter => "Cover letter",
                    ChecklistItemType.Portfolio => "Portfolio evidence",
                    ChecklistItemType.References => "Reviewed references",
                    ChecklistItemType.Questions => "Application questions",
                    ChecklistItemType.RequiredForms => "Required forms",
                    _ => "Supporting files"
                },
                type is ChecklistItemType.Cv or ChecklistItemType.RequiredForms,
                false,
                null,
                null))
            .ToArray();

        var app = new CareerApplication(
            $"app-{opportunity.Id}", opportunity.Id, ApplicationState.Preparing, now,
            null, "Not submitted", null, null, checklist, [],
            [new ApplicationStageEvent(now, null, ApplicationState.Preparing, "Application preparation created explicitly.")],
            [], [], []);
        return new(true, app, "Application preparation created.");
    }

    public ApplicationActionResult SetChecklistItem(
        CareerApplication application,
        string itemId,
        bool complete,
        DateTimeOffset now)
    {
        var found = false;
        var checklist = application.Checklist.Select(item =>
        {
            if (item.Id != itemId) return item;
            found = true;
            return item with { IsComplete = complete, CompletedUtc = complete ? now : null };
        }).ToArray();
        if (!found) return new(false, application, "Checklist item was not found.");

        var requiredReady = checklist.Where(x => x.IsRequired).All(x => x.IsComplete);
        var target = requiredReady ? ApplicationState.ReadyToSubmit : ApplicationState.Preparing;
        var timeline = application.Timeline;
        if (target != application.State)
            timeline = timeline.Concat([new ApplicationStageEvent(now, application.State, target, "Readiness recalculated from explicit checklist state.")]).ToArray();

        return new(true, application with { Checklist = checklist, State = target, Timeline = timeline }, "Checklist updated explicitly.");
    }

    public ApplicationActionResult MarkSubmitted(
        CareerApplication application,
        string channel,
        string confirmationReference,
        bool confirmed,
        DateTimeOffset now)
    {
        if (!confirmed) return new(false, application, "Submission confirmation is required.");
        if (application.State != ApplicationState.ReadyToSubmit)
            return new(false, application, "Application is not explicitly ready to submit.");
        if (string.IsNullOrWhiteSpace(channel))
            return new(false, application, "Submission channel is required.");

        var timeline = application.Timeline.Concat(
        [
            new ApplicationStageEvent(now, application.State, ApplicationState.Submitted, "User recorded an external submission; LifeOS did not submit it.")
        ]).ToArray();

        return new(true, application with
        {
            State = ApplicationState.Submitted,
            SubmittedUtc = now,
            SubmissionChannel = channel,
            ConfirmationReference = confirmationReference,
            Timeline = timeline
        }, "External submission recorded.");
    }

    public CareerDashboardSummary BuildDashboard(
        IReadOnlyList<CareerOpportunity> opportunities,
        IReadOnlyList<CareerApplication> applications,
        DateTimeOffset now)
    {
        return new(
            opportunities.Count(x => x.Stage is not OpportunityStage.Archived and not OpportunityStage.Declined and not OpportunityStage.Rejected),
            opportunities.Count(x => x.ClosingUtc is not null && x.ClosingUtc >= now && x.ClosingUtc <= now.AddDays(7)),
            applications.Count(x => x.State is not ApplicationState.Closed and not ApplicationState.Withdrawn),
            applications.Sum(x => x.Interviews.Count(i => i.StartsUtc >= now)),
            applications.Sum(x => x.FollowUps.Count(f => f.DueUtc < now && f.State is not FollowUpState.Completed)),
            applications.Sum(x => x.FollowUps.Count(f => f.State == FollowUpState.WaitingOn)));
    }

    public FollowUpState CalculateFollowUpState(CareerFollowUp followUp, DateTimeOffset now) =>
        followUp.State == FollowUpState.Completed ? FollowUpState.Completed :
        followUp.State == FollowUpState.WaitingOn ? FollowUpState.WaitingOn :
        followUp.DueUtc < now ? FollowUpState.Overdue :
        followUp.DueUtc <= now.AddDays(1) ? FollowUpState.Due :
        FollowUpState.Scheduled;
}
