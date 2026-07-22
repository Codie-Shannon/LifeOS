namespace LifeOS.Core.CareerStudio;

public enum OpportunityStage
{
    Discovered, Reviewing, Interested, Preparing, Applied, Interviewing,
    Offer, Accepted, Declined, Rejected, Withdrawn, Paused, Archived
}

public enum OpportunitySourceType
{
    DirectEmployer, Referral, Recruiter, JobBoard, Email, Networking, ManualCapture
}

public enum WorkMode { OnSite, Hybrid, Remote, Flexible }
public enum EmploymentType { Permanent, FixedTerm, Contract, Casual, PartTime, FullTime, Internship }
public enum RequirementType { Skill, Experience, Qualification, Location, Availability, ApplicationMaterial }
public enum CandidateReviewState { AwaitingReview, Approved, Rejected, Deferred }
public enum PriorityLevel { Low, Normal, High, Urgent }

public sealed record Employer(string Id, string Name, string? Website, string? Location);
public sealed record RecruiterContact(string Id, string DisplayName, string? Organisation, string? Email, string? Phone);
public sealed record OpportunitySource(
    string Id,
    OpportunitySourceType Type,
    string DisplayName,
    string? Reference,
    DateTimeOffset CapturedUtc,
    DateTimeOffset FreshnessUtc);

public sealed record RoleRequirement(
    string Id,
    RequirementType Type,
    string Description,
    bool IsRequired,
    string? EvidenceLinkId);

public sealed record OpportunityFit(
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Gaps,
    IReadOnlyList<string> Blockers,
    IReadOnlyList<string> EvidenceLinks,
    DateTimeOffset ReviewedUtc,
    string ReviewedBy,
    string Explanation)
{
    public bool IsEvidenceBacked => Strengths.Count == 0 || EvidenceLinks.Count > 0;
}

public sealed record CareerNextAction(
    string Id,
    string Description,
    DateTimeOffset? DueUtc,
    bool IsCompleted,
    string? LinkedRecordId);

public sealed record OpportunityHistory(
    DateTimeOffset OccurredUtc,
    string Action,
    OpportunityStage? FromStage,
    OpportunityStage? ToStage,
    string SafeSummary);

public sealed record CareerOpportunity(
    string Id,
    string Title,
    Employer Employer,
    RecruiterContact? Recruiter,
    OpportunitySource Source,
    OpportunityStage Stage,
    string RoleSummary,
    string Location,
    WorkMode WorkMode,
    EmploymentType EmploymentType,
    string SalaryOrRateContext,
    DateTimeOffset CapturedUtc,
    DateTimeOffset? ClosingUtc,
    DateTimeOffset FreshnessUtc,
    PriorityLevel Priority,
    IReadOnlyList<RoleRequirement> Requirements,
    OpportunityFit? Fit,
    IReadOnlyList<string> PeopleLinks,
    IReadOnlyList<string> WorkLinks,
    IReadOnlyList<string> ProjectLinks,
    IReadOnlyList<string> DocumentLinks,
    IReadOnlyList<string> PortfolioEvidenceLinks,
    IReadOnlyList<CareerNextAction> NextActions,
    IReadOnlyList<OpportunityHistory> History);

public sealed record ImportedOpportunityCandidate(
    string Id,
    string IntegrationInboxItemId,
    string EmployerName,
    string RoleTitle,
    string SourceReference,
    DateTimeOffset CapturedUtc,
    CandidateReviewState ReviewState,
    string ReviewReason);

public sealed record DuplicateOpportunityCandidate(
    string Id,
    string ExistingOpportunityId,
    string CandidateOpportunityId,
    decimal Confidence,
    IReadOnlyList<string> Signals,
    CandidateReviewState ReviewState);

public sealed record StageTransitionResult(bool Applied, CareerOpportunity Opportunity, string Message);
