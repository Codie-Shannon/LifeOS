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
public enum CvExportFormat { Pdf, Docx }
public enum CvPageDensity { Spacious, Balanced, Compact }

public sealed record CvDocumentLayout(
    string FontFamily,
    double FontScale,
    double PageMarginMillimetres,
    string AccentHex,
    CvPageDensity Density,
    bool ShowSectionRules)
{
    public static CvDocumentLayout Default =>
        new("Aptos", 1.0, 18, "#315E91", CvPageDensity.Balanced, true);
}

public sealed record CvTemplateDefinition(
    string Id,
    string Name,
    string Description,
    bool AtsFriendly,
    CvDocumentLayout Layout);

public sealed record CvReadabilityCheck(
    string Code,
    string Label,
    string Detail,
    bool Passed,
    CvValidationSeverity Severity);

public sealed record CvReadabilityReview(
    int Score,
    int EstimatedPages,
    IReadOnlyList<CvReadabilityCheck> Checks)
{
    public bool CanExport =>
        Checks.All(check => check.Severity != CvValidationSeverity.Blocking || check.Passed);
}

public sealed record CvVersionSnapshot(
    int Version,
    DateTimeOffset SavedUtc,
    string Label,
    string TemplateId,
    int EnabledSectionCount);

public sealed record CvExportArtifact(
    CvExportFormat Format,
    string SuggestedFileName,
    string MediaType,
    byte[] Content,
    int SourceVersion,
    DateTimeOffset CreatedUtc);

public sealed record CvBuilderEntry(
    string Id,
    string Title,
    string Organization,
    string City,
    DateTime? StartDate,
    DateTime? EndDate,
    bool IsCurrent,
    string Description,
    string RichContent = "");

public sealed record CvBuilderSection(
    string Id,
    CvSectionKind Kind,
    string Heading,
    string Content,
    int Order,
    bool IsEnabled,
    IReadOnlyList<string> SourceFactIds,
    string Subtitle = "",
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    bool ShowDateRange = false,
    string RichContent = "",
    bool ShowSubtitle = false,
    IReadOnlyList<CvBuilderEntry>? Entries = null);

public sealed record CvBuilderDocument(
    string Id,
    string Name,
    string TargetRole,
    string TemplateId,
    IReadOnlyList<CvBuilderSection> Sections,
    CvBuilderStep ActiveStep,
    int Version,
    DateTimeOffset UpdatedUtc,
    bool IsAutosaved,
    CvDocumentLayout? Layout = null)
{
    public CvDocumentLayout EffectiveLayout => Layout ?? CvDocumentLayout.Default;

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
