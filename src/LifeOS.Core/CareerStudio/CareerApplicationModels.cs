namespace LifeOS.Core.CareerStudio;

public enum ApplicationState
{
    Preparing, ReadyToSubmit, Submitted, Acknowledged, InterviewScheduled,
    InterviewComplete, OfferReceived, Closed, Withdrawn
}

public enum ChecklistItemType { Cv, CoverLetter, Portfolio, References, Questions, RequiredForms, SupportingFiles }
public enum FollowUpState { Scheduled, Due, Overdue, WaitingOn, Completed, Deferred }
public enum InterviewFormat { InPerson, Phone, Video, Technical, Panel, Other }

public sealed record ApplicationStageEvent(
    DateTimeOffset OccurredUtc,
    ApplicationState? FromState,
    ApplicationState ToState,
    string SafeSummary);

public sealed record ApplicationChecklistItem(
    string Id,
    ChecklistItemType Type,
    string Label,
    bool IsRequired,
    bool IsComplete,
    DateTimeOffset? CompletedUtc,
    string? EvidenceLinkId);

public sealed record ApplicationMaterialLink(string Id, ChecklistItemType Type, string DocumentId, string DisplayName);

public sealed record InterviewRecord(
    string Id,
    DateTimeOffset StartsUtc,
    InterviewFormat Format,
    IReadOnlyList<string> AttendeeDisplayNames,
    bool PreparationComplete,
    IReadOnlyList<string> PostInterviewActions,
    string? CalendarContextId);

public sealed record CareerFollowUp(
    string Id,
    DateTimeOffset DueUtc,
    FollowUpState State,
    string Description,
    string? CommunicationContextId);

public sealed record CareerApplication(
    string Id,
    string OpportunityId,
    ApplicationState State,
    DateTimeOffset CreatedUtc,
    DateTimeOffset? SubmittedUtc,
    string SubmissionChannel,
    string? ConfirmationReference,
    DateTimeOffset? NextFollowUpUtc,
    IReadOnlyList<ApplicationChecklistItem> Checklist,
    IReadOnlyList<ApplicationMaterialLink> Materials,
    IReadOnlyList<ApplicationStageEvent> Timeline,
    IReadOnlyList<InterviewRecord> Interviews,
    IReadOnlyList<CareerFollowUp> FollowUps,
    IReadOnlyList<string> EvidenceLinks);

public sealed record ApplicationActionResult(bool Applied, CareerApplication Application, string Message);
public sealed record CareerDashboardSummary(
    int ActiveOpportunities,
    int ClosingSoon,
    int ActiveApplications,
    int Interviews,
    int OverdueFollowUps,
    int WaitingOn);
