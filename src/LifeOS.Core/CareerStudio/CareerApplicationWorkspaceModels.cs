namespace LifeOS.Core.CareerStudio;

public enum CoverLetterBuilderStep
{
    Opportunity,
    Evidence,
    Draft,
    Review
}

public sealed record CoverLetterDocument(
    string Id,
    string Name,
    string OpportunityId,
    string CvDocumentId,
    IReadOnlyList<CoverLetterSection> Sections,
    CoverLetterBuilderStep ActiveStep,
    int Version,
    DateTimeOffset UpdatedUtc,
    bool IsAutosaved,
    bool IncludeContactDetails,
    bool ContactDetailsConfirmed)
{
    public IReadOnlyList<string> SourceFactIds => Sections
        .SelectMany(section => section.SourceFactIds)
        .Distinct(StringComparer.Ordinal)
        .ToArray();
}

public sealed record CoverLetterValidationIssue(
    string Code,
    string Message,
    CvValidationSeverity Severity,
    string? SectionId = null);

public sealed record CoverLetterReview(
    IReadOnlyList<CoverLetterValidationIssue> Issues,
    int AcceptedSuggestionCount,
    int SourceFactCount)
{
    public bool CanExport => Issues.All(issue =>
        issue.Severity != CvValidationSeverity.Blocking);
}

public sealed record ApplicationPackDocumentLink(
    CareerDocumentKind Kind,
    string DocumentId,
    int SourceVersion,
    MaterialFreshnessState Freshness,
    DateTimeOffset LinkedUtc);

public sealed record CareerApplicationPack(
    string Id,
    string OpportunityId,
    string ApplicationId,
    IReadOnlyList<ApplicationPackDocumentLink> Documents,
    int Version,
    DateTimeOffset UpdatedUtc,
    bool Reviewed)
{
    public bool IsReady =>
        Reviewed &&
        Documents.Any(link =>
            link.Kind == CareerDocumentKind.Cv &&
            link.Freshness == MaterialFreshnessState.Current) &&
        Documents.Any(link =>
            link.Kind == CareerDocumentKind.CoverLetter &&
            link.Freshness == MaterialFreshnessState.Current);
}

public sealed record CareerApplicationWorkspace(
    int SchemaVersion,
    IReadOnlyList<CareerOpportunity> Opportunities,
    IReadOnlyList<CoverLetterDocument> CoverLetters,
    IReadOnlyList<CareerApplication> Applications,
    IReadOnlyList<CareerApplicationPack> Packs,
    IReadOnlyList<CareerFact> Facts,
    string ActiveOpportunityId,
    string ActiveCoverLetterId)
{
    public const int CurrentSchemaVersion = 1;

    public static CareerApplicationWorkspace Empty => new(
        CurrentSchemaVersion,
        [],
        [],
        [],
        [],
        [],
        string.Empty,
        string.Empty);
}
