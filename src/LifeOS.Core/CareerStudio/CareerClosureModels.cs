namespace LifeOS.Core.CareerStudio;

public enum CareerFollowUpType { ApplicationAcknowledgement, RecruiterResponse, InterviewOutcome, ReferenceRequest, PortfolioDelivery, PromisedUpdate, GeneralCheckIn }
public enum CareerFollowUpStatus { Planned, Due, Overdue, Waiting, Completed, Cancelled, Deferred }
public enum ReferencePermissionState { Unknown, Requested, Granted, Declined, Expired }
public enum CareerPrivacyState { Private, Redacted, Shareable }

public sealed record FollowUpEvent(DateTimeOffset OccurredUtc, string Action, string SafeSummary);
public sealed record CareerFollowUpPlan(string Id, string Title, CareerFollowUpType Type, DateTimeOffset DueUtc, string Owner, string RelatedRecordId, string Channel, string DraftNote, CareerFollowUpStatus Status, string? Outcome, IReadOnlyList<FollowUpEvent> History);
public sealed record WaitingOnRecord(string Id, string RelatedRecordId, string WaitingOn, DateTimeOffset SinceUtc, DateTimeOffset? ReviewUtc, string SafeNote);
public sealed record ReferencePermission(ReferencePermissionState State, DateTimeOffset? GrantedUtc, DateTimeOffset? ReviewUtc, string EvidenceId);
public sealed record ReferenceUsage(string ApplicationPackId, string OpportunityId, DateTimeOffset UsedUtc, string SafePurpose);
public sealed record ReferenceHistory(DateTimeOffset OccurredUtc, string Action, string SafeSummary);
public sealed record ReferenceContact(string Id, string PeopleContactId, string DisplayName, string Relationship, string Role, string RelevantWork, ReferencePermission Permission, string PreferredContactMethod, string Availability, CareerPrivacyState PrivacyState, string? RedactedContact, IReadOnlyList<ReferenceUsage> UsageHistory, IReadOnlyList<ReferenceHistory> History, IReadOnlyList<string> ReadinessWarnings)
{
    public bool CanUseInApplicationPack => Permission.State == ReferencePermissionState.Granted && ReadinessWarnings.Count == 0;
}

public sealed record InterviewQuestionToAsk(string Id, string InterviewId, int Order, string Category, string Prompt, string Notes, bool Answered);
public sealed record QuestionsToAskPlan(string InterviewId, string OpportunityTitle, DateTimeOffset InterviewUtc, string Notes, IReadOnlyList<InterviewQuestionToAsk> Questions);

public sealed record CareerReviewPeriod(DateTimeOffset StartUtc, DateTimeOffset EndUtc);
public sealed record PipelineSummary(int OpportunitiesDiscovered, int ApplicationsPrepared, int ApplicationsSubmitted, int Interviews, int Offers, int ClosedOutcomes);
public sealed record FollowUpPerformance(int CompletedOnTime, int Overdue, int Waiting, double AverageResponseHours);
public sealed record CoverageDrillDown(string Label, string Status, string RecordId, string SafeDetail);
public sealed record EvidenceCoverageSummary(int TrustedFacts, int TotalFacts, int PortfolioWithEvidence, int TotalPortfolio, int ReadyReferences, int TotalReferences, int ReadyPacks, int TotalPacks, IReadOnlyList<CoverageDrillDown> DrillDown);
public sealed record CareerMetricSnapshot(CareerReviewPeriod Period, string DataState, PipelineSummary Pipeline, FollowUpPerformance FollowUps, EvidenceCoverageSummary Coverage, IReadOnlyDictionary<string,int> SourceBreakdown, IReadOnlyDictionary<string,int> RoleFamilyBreakdown, IReadOnlyDictionary<string,int> WorkModeBreakdown, IReadOnlyDictionary<string,int> StageBreakdown);
public sealed record CareerClosureProof(IReadOnlyList<CareerFollowUpPlan> FollowUps, IReadOnlyList<WaitingOnRecord> WaitingOn, IReadOnlyList<ReferenceContact> References, QuestionsToAskPlan QuestionsToAsk, CareerMetricSnapshot Review);
