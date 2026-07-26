namespace LifeOS.Core.CareerStudio;

public enum CareerDocumentKind { Cv, CoverLetter }
public enum CvBuilderStep { Content, Sections, TargetRole, Review }
public enum CvSectionKind
{
    Contact,
    Profile,
    Employment,
    Education,
    Skills,
    Projects,
    Certifications,
    Achievements,
    Custom
}

public enum CvValidationSeverity { Information, Warning, Blocking }

public sealed record CvBuilderSection(
    string Id,
    CvSectionKind Kind,
    string Heading,
    string Content,
    int Order,
    bool IsEnabled,
    IReadOnlyList<string> SourceFactIds);

public sealed record CvBuilderDocument(
    string Id,
    string Name,
    string TargetRole,
    string TemplateId,
    IReadOnlyList<CvBuilderSection> Sections,
    CvBuilderStep ActiveStep,
    int Version,
    DateTimeOffset UpdatedUtc,
    bool IsAutosaved)
{
    public IReadOnlyList<CvBuilderSection> VisibleSections =>
        Sections.Where(section => section.IsEnabled).OrderBy(section => section.Order).ToArray();
}

public sealed record CvValidationIssue(
    string Code,
    string Message,
    CvValidationSeverity Severity,
    string? SectionId = null);

public sealed record CvBuilderReview(
    IReadOnlyList<CvValidationIssue> Issues,
    int TrustedFactCount,
    int TotalSourceFactCount,
    int CompletedRequiredSections)
{
    public bool CanExport => Issues.All(issue => issue.Severity != CvValidationSeverity.Blocking);
}

public sealed record CvBuilderWorkspace(
    IReadOnlyList<CvBuilderDocument> Documents,
    string ActiveDocumentId,
    CvBuilderReview Review);
