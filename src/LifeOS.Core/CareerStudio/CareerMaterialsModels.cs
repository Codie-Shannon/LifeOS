namespace LifeOS.Core.CareerStudio;

public enum CareerTrustState { Draft, SourceBacked, UserAccepted, Stale, Rejected }
public enum CareerOwnerReviewState { Unreviewed, Reviewed, Accepted, Rejected }
public enum PortfolioPrivacyState { Private, Redacted, Shareable }
public enum MaterialFreshnessState { Current, Stale, Missing }

public sealed record CareerFact(
    string Id,
    string Category,
    string FactualValue,
    string SourceId,
    CareerTrustState TrustState,
    CareerOwnerReviewState OwnerReviewState,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    IReadOnlyList<string>? EvidenceIds = null)
{
    public IReadOnlyList<string> Evidence => EvidenceIds ?? Array.Empty<string>();
    public bool IsTrusted => TrustState is CareerTrustState.SourceBacked or CareerTrustState.UserAccepted;
    public bool HasProvenance => !string.IsNullOrWhiteSpace(SourceId);
}

public sealed record SkillEvidence(string Skill, string CareerFactId, string EvidenceId, CareerTrustState TrustState);
public sealed record EmploymentRecord(string Id, string Employer, string Role, DateOnly From, DateOnly? To, IReadOnlyList<string> CareerFactIds);
public sealed record EducationRecord(string Id, string Provider, string Qualification, DateOnly? Completed, string SourceId);
public sealed record CertificationRecord(string Id, string Name, string Issuer, DateOnly? Issued, DateOnly? Expires, string SourceId);
public sealed record AchievementRecord(string Id, string Title, string CareerFactId, string EvidenceId);
public sealed record ProjectEvidence(string Id, string ProjectName, string Summary, string Role, IReadOnlyList<string> Technologies, IReadOnlyList<string> EvidenceIds, PortfolioPrivacyState PrivacyState);
public sealed record CVProfile(string Id, string Name, string SummaryDraft, IReadOnlyList<string> IncludedCareerFactIds);
public sealed record CVBullet(string Id, string Text, string CareerFactId, CareerTrustState TrustState);
public sealed record CVVariantVersion(int Version, DateTimeOffset AcceptedUtc, IReadOnlyList<string> IncludedFactIds, IReadOnlyList<string> BulletIds, string ExportDerivativeId);
public sealed record CVVariant(string Id, string Name, string Focus, IReadOnlyList<string> IncludedFactIds, IReadOnlyList<CVBullet> Bullets, IReadOnlyList<CVVariantVersion> Versions);
public sealed record PortfolioItem(string Id, string Title, string Summary, string Role, IReadOnlyList<string> Technologies, IReadOnlyList<string> ScreenshotIds, IReadOnlyList<string> DocumentIds, string? RepositoryLink, IReadOnlyList<string> Outcomes, PortfolioPrivacyState PrivacyState);
public sealed record RequirementEvidenceMatch(string RequirementId, string Requirement, IReadOnlyList<string> MatchedFactIds, IReadOnlyList<string> EvidenceIds, bool IsRequired)
{
    public bool IsSupported => MatchedFactIds.Count > 0 && EvidenceIds.Count > 0;
}
public sealed record CareerMaterialReview(IReadOnlyList<RequirementEvidenceMatch> Matches, IReadOnlyList<string> UnsupportedClaims, IReadOnlyList<string> MissingProof, IReadOnlyList<string> StaleFactIds)
{
    public bool CanExport => UnsupportedClaims.Count == 0 && MissingProof.Count == 0;
}
public sealed record CVExportDerivative(string Id, string CVVariantId, int Version, DateTimeOffset CreatedUtc, string Format, IReadOnlyList<string> SourceFactIds);
public sealed record CareerMaterialsProof(
    IReadOnlyList<CareerFact> Facts,
    IReadOnlyList<SkillEvidence> Skills,
    IReadOnlyList<EmploymentRecord> Employment,
    IReadOnlyList<ProjectEvidence> Projects,
    IReadOnlyList<CVVariant> Variants,
    IReadOnlyList<PortfolioItem> Portfolio,
    CareerMaterialReview Review,
    CVExportDerivative Export);
