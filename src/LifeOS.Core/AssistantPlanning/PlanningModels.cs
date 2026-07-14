namespace LifeOS.Core.AssistantPlanning;

using LifeOS.Core.Assistant;

public enum PlanningBlockType { Goal, Evidence, Constraint, Step, Dependency, Risk, Decision, Verification, Handoff }
public enum PlanningBlockStatus { Proposed, NeedsInput, Blocked, ReadyForReview }

public sealed record PlanningClaimSource(
    string RecordId,
    AssistantSourceType Source,
    string Title,
    DateTimeOffset Timestamp,
    string Provenance);

public sealed record PlanningBlock(
    Guid BlockId,
    PlanningBlockType Type,
    string Title,
    string Content,
    string Rationale,
    PlanningBlockStatus Status,
    IReadOnlyList<PlanningClaimSource> Sources,
    bool IsAssumption = false,
    bool HasConflict = false,
    bool IsStale = false,
    bool HasMissingData = false)
{
    public bool IsExecutable => false;
}

public sealed record ReviewOnlyPlan(
    Guid PlanId,
    DateTimeOffset CreatedAt,
    string SourceQuestion,
    IReadOnlyList<PlanningBlock> Blocks,
    IReadOnlyList<string> Assumptions,
    IReadOnlyList<string> MissingData,
    IReadOnlyList<string> Conflicts,
    PlanningBlockStatus Readiness,
    string ReadinessReason)
{
    public bool Executable => false;
    public bool ReviewOnly => true;
}

public sealed record PlanningRequest(string Question, AssistantResponse Response, DateTimeOffset? Now = null);

public enum PlanningReviewSurface { AssistantReviewQueue, WeeklyCloseOutReview, EvidenceReview }

public sealed record PlanningHandoffPreview(
    Guid PreviewId,
    Guid PlanId,
    PlanningReviewSurface Target,
    DateTimeOffset PreviewedAt,
    string OriginalQuestion,
    IReadOnlyList<PlanningBlock> Blocks,
    IReadOnlyList<string> Assumptions,
    IReadOnlyList<string> MissingData,
    IReadOnlyList<string> Conflicts,
    string Provenance,
    bool Executable = false);

public sealed record PlanningReviewArtifact(
    Guid ArtifactId,
    Guid PlanId,
    PlanningReviewSurface Target,
    DateTimeOffset CreatedAt,
    string OriginalQuestion,
    IReadOnlyList<PlanningBlock> Blocks,
    IReadOnlyList<string> Assumptions,
    IReadOnlyList<string> MissingData,
    IReadOnlyList<string> Conflicts,
    string Provenance,
    bool Executable = false,
    string Status = "ReviewOnly");
