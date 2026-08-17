namespace LifeOS.Core.CareerStudio;

public sealed class CareerApplicationWorkspaceService
{
    private readonly CareerDocumentLayoutService _exportService = new();

    public CareerOpportunity CreateManualOpportunity(
        string id,
        string title,
        string employer,
        string roleSummary,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(employer);

        return new CareerOpportunity(
            id.Trim(),
            title.Trim(),
            new Employer($"employer-{id.Trim()}", employer.Trim(), null, null),
            null,
            new OpportunitySource(
                $"source-{id.Trim()}",
                OpportunitySourceType.ManualCapture,
                "Manual capture",
                null,
                now,
                now),
            OpportunityStage.Reviewing,
            roleSummary.Trim(),
            "Not supplied",
            WorkMode.Flexible,
            EmploymentType.FullTime,
            "Not supplied",
            now,
            null,
            now,
            PriorityLevel.Normal,
            [],
            null,
            [],
            [],
            [],
            [],
            [],
            [new CareerNextAction(
                $"next-{id.Trim()}",
                "Review the role requirements and link trusted evidence.",
                null,
                false,
                id.Trim())],
            [new OpportunityHistory(
                now,
                "Captured",
                null,
                OpportunityStage.Reviewing,
                "Opportunity captured manually in LifeOS.")]);
    }

    public CoverLetterDocument CreateDraft(
        string id,
        string name,
        CareerOpportunity opportunity,
        CvBuilderDocument cv,
        IReadOnlyList<CareerFact> facts,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(opportunity);
        ArgumentNullException.ThrowIfNull(cv);

        CareerFact[] trusted = facts
            .Where(fact => fact.IsTrusted && fact.HasProvenance)
            .Take(4)
            .ToArray();
        IReadOnlyList<string> sourceIds = trusted.Select(fact => fact.Id).ToArray();
        string evidenceText = trusted.Length == 0
            ? string.Empty
            : string.Join(" ", trusted.Select(fact => fact.FactualValue));

        return new CoverLetterDocument(
            id.Trim(),
            name.Trim(),
            opportunity.Id,
            cv.Id,
            [
                new CoverLetterSection(
                    "opening",
                    "Opening",
                    $"I am writing to apply for the {opportunity.Title} opportunity with {opportunity.Employer.Name}.",
                    DraftSectionState.Generated,
                    []),
                new CoverLetterSection(
                    "evidence",
                    "Relevant evidence",
                    evidenceText,
                    DraftSectionState.Generated,
                    sourceIds),
                new CoverLetterSection(
                    "motivation",
                    "Role motivation",
                    string.Empty,
                    DraftSectionState.Manual,
                    []),
                new CoverLetterSection(
                    "closing",
                    "Closing",
                    "Thank you for considering my application.",
                    DraftSectionState.Manual,
                    [])
            ],
            CoverLetterBuilderStep.Evidence,
            1,
            now,
            true,
            false,
            false);
    }

    public CoverLetterDocument UpdateSection(
        CoverLetterDocument document,
        string sectionId,
        string text,
        DateTimeOffset now)
    {
        bool found = false;
        CoverLetterSection[] sections = document.Sections.Select(section =>
        {
            if (!string.Equals(section.Id, sectionId, StringComparison.Ordinal))
                return section;
            found = true;
            return section with
            {
                Text = text.Trim(),
                State = DraftSectionState.Manual
            };
        }).ToArray();
        if (!found)
            throw new ArgumentException("The cover-letter section was not found.", nameof(sectionId));

        return Autosave(document, sections, now);
    }

    public CoverLetterDocument SetSuggestionState(
        CoverLetterDocument document,
        string sectionId,
        DraftSectionState state,
        DateTimeOffset now)
    {
        if (state is not DraftSectionState.Accepted and not DraftSectionState.Rejected)
            throw new ArgumentOutOfRangeException(nameof(state), "Suggestions can only be accepted or rejected.");

        bool found = false;
        CoverLetterSection[] sections = document.Sections.Select(section =>
        {
            if (!string.Equals(section.Id, sectionId, StringComparison.Ordinal))
                return section;
            found = true;
            if (section.State == DraftSectionState.Stale && state == DraftSectionState.Accepted)
                throw new InvalidOperationException("A stale suggestion must be refreshed before acceptance.");
            if (section.State is not DraftSectionState.Generated and not DraftSectionState.Accepted and not DraftSectionState.Rejected)
                throw new InvalidOperationException("Only generated suggestions have an acceptance state.");
            return section with { State = state };
        }).ToArray();
        if (!found)
            throw new ArgumentException("The cover-letter section was not found.", nameof(sectionId));

        return Autosave(document, sections, now);
    }

    public CoverLetterDocument SetContactDetails(
        CoverLetterDocument document,
        bool include,
        bool confirmed,
        DateTimeOffset now) => document with
        {
            IncludeContactDetails = include,
            ContactDetailsConfirmed = include && confirmed,
            Version = document.Version + 1,
            UpdatedUtc = now,
            IsAutosaved = true
        };

    public CoverLetterReview Review(
        CoverLetterDocument document,
        CareerOpportunity opportunity,
        CvBuilderDocument? cv,
        IReadOnlyList<CareerFact> facts) => Review(
            document,
            opportunity,
            cv?.Id,
            facts);

    public CoverLetterReview Review(
        CoverLetterDocument document,
        CareerOpportunity opportunity,
        string? linkedCvDocumentId,
        IReadOnlyList<CareerFact> facts)
    {
        List<CoverLetterValidationIssue> issues = [];
        if (!string.Equals(document.OpportunityId, opportunity.Id, StringComparison.Ordinal))
            issues.Add(Block("opportunity-link", "The letter is linked to a different opportunity."));
        if (string.IsNullOrWhiteSpace(linkedCvDocumentId) ||
            !string.Equals(document.CvDocumentId, linkedCvDocumentId, StringComparison.Ordinal))
            issues.Add(Block("cv-link", "Link a current CV before export."));

        IReadOnlyDictionary<string, CareerFact> factIndex = facts
            .Where(fact => !string.IsNullOrWhiteSpace(fact.Id))
            .GroupBy(fact => fact.Id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        foreach (CoverLetterSection section in document.Sections)
        {
            if (string.IsNullOrWhiteSpace(section.Text))
                issues.Add(Block("empty-section", $"{section.Heading} is empty.", section.Id));
            if (section.State == DraftSectionState.Generated)
                issues.Add(Block("unreviewed-suggestion", $"Review the generated {section.Heading} suggestion.", section.Id));
            if (section.State is DraftSectionState.Rejected or DraftSectionState.Stale)
                issues.Add(Block("rejected-or-stale", $"Resolve the {section.Heading} section before export.", section.Id));

            foreach (string sourceId in section.SourceFactIds)
            {
                if (!factIndex.TryGetValue(sourceId, out CareerFact? fact) ||
                    !fact.IsTrusted ||
                    !fact.HasProvenance)
                {
                    issues.Add(Block(
                        "unsupported-source",
                        $"{section.Heading} contains an unsupported source claim.",
                        section.Id));
                }
            }
        }

        if (document.IncludeContactDetails && !document.ContactDetailsConfirmed)
            issues.Add(Block("contact-confirmation", "Confirm contact-detail inclusion before export."));
        if (!document.SourceFactIds.Any())
            issues.Add(new CoverLetterValidationIssue(
                "no-linked-evidence",
                "No trusted evidence is linked yet.",
                CvValidationSeverity.Warning));

        return new CoverLetterReview(
            issues,
            document.Sections.Count(section => section.State == DraftSectionState.Accepted),
            document.SourceFactIds.Count);
    }

    public CvExportArtifact Export(
        CoverLetterDocument document,
        CoverLetterReview review,
        CareerOpportunity opportunity,
        CvExportFormat format,
        DateTimeOffset now)
    {
        if (!review.CanExport)
            throw new InvalidOperationException("Resolve blocking cover-letter checks before export.");

        IReadOnlyList<(string Heading, string Content)> sections = document.Sections
            .Where(section => section.State is DraftSectionState.Accepted or DraftSectionState.Manual)
            .Select(section => (section.Heading, section.Text))
            .ToArray();
        return _exportService.ExportTextDocument(
            document.Name,
            $"{opportunity.Title} - {opportunity.Employer.Name}",
            sections,
            document.Version,
            format,
            now);
    }

    public CareerApplicationPack CreatePack(
        string id,
        CareerOpportunity opportunity,
        CareerApplication application,
        CvBuilderDocument cv,
        CoverLetterDocument coverLetter,
        DateTimeOffset now)
    {
        if (!string.Equals(application.OpportunityId, opportunity.Id, StringComparison.Ordinal) ||
            !string.Equals(coverLetter.OpportunityId, opportunity.Id, StringComparison.Ordinal))
            throw new InvalidOperationException("Application-pack records must belong to the same opportunity.");
        if (!string.Equals(coverLetter.CvDocumentId, cv.Id, StringComparison.Ordinal))
            throw new InvalidOperationException("The cover letter must link the selected CV.");

        return new CareerApplicationPack(
            id,
            opportunity.Id,
            application.Id,
            [
                new ApplicationPackDocumentLink(
                    CareerDocumentKind.Cv,
                    cv.Id,
                    cv.Version,
                    MaterialFreshnessState.Current,
                    now),
                new ApplicationPackDocumentLink(
                    CareerDocumentKind.CoverLetter,
                    coverLetter.Id,
                    coverLetter.Version,
                    MaterialFreshnessState.Current,
                    now)
            ],
            1,
            now,
            false);
    }

    public CareerApplicationPack ReviewPack(
        CareerApplicationPack pack,
        CvBuilderDocument? cv,
        CvBuilderReview cvReview,
        CoverLetterDocument? coverLetter,
        CoverLetterReview coverLetterReview,
        DateTimeOffset now)
    {
        if (cv is null ||
            coverLetter is null ||
            !cvReview.CanExport ||
            !coverLetterReview.CanExport)
            throw new InvalidOperationException("Current reviewed documents are required before the pack can be approved.");

        ApplicationPackDocumentLink[] links = pack.Documents.Select(link => link.Kind switch
        {
            CareerDocumentKind.Cv when link.DocumentId == cv.Id => link with
            {
                SourceVersion = cv.Version,
                Freshness = MaterialFreshnessState.Current,
                LinkedUtc = now
            },
            CareerDocumentKind.CoverLetter when link.DocumentId == coverLetter.Id => link with
            {
                SourceVersion = coverLetter.Version,
                Freshness = MaterialFreshnessState.Current,
                LinkedUtc = now
            },
            _ => link with { Freshness = MaterialFreshnessState.Stale }
        }).ToArray();

        return pack with
        {
            Documents = links,
            Version = pack.Version + 1,
            UpdatedUtc = now,
            Reviewed = true
        };
    }

    public CareerApplicationPack RefreshFreshness(
        CareerApplicationPack pack,
        CvBuilderDocument? cv,
        CoverLetterDocument? coverLetter,
        DateTimeOffset now)
    {
        ApplicationPackDocumentLink[] links = pack.Documents.Select(link =>
        {
            int? currentVersion = link.Kind switch
            {
                CareerDocumentKind.Cv when cv?.Id == link.DocumentId => cv.Version,
                CareerDocumentKind.CoverLetter when coverLetter?.Id == link.DocumentId => coverLetter.Version,
                _ => null
            };
            return link with
            {
                Freshness = currentVersion is null
                    ? MaterialFreshnessState.Missing
                    : currentVersion == link.SourceVersion
                        ? MaterialFreshnessState.Current
                        : MaterialFreshnessState.Stale
            };
        }).ToArray();
        return pack with
        {
            Documents = links,
            UpdatedUtc = now,
            Reviewed = pack.Reviewed && links.All(link => link.Freshness == MaterialFreshnessState.Current)
        };
    }

    private static CoverLetterDocument Autosave(
        CoverLetterDocument document,
        IReadOnlyList<CoverLetterSection> sections,
        DateTimeOffset now) => document with
        {
            Sections = sections,
            Version = document.Version + 1,
            UpdatedUtc = now,
            IsAutosaved = true
        };

    private static CoverLetterValidationIssue Block(
        string code,
        string message,
        string? sectionId = null) => new(
            code,
            message,
            CvValidationSeverity.Blocking,
            sectionId);
}

public static class CareerApplicationWorkspaceProofData
{
    public static CareerApplicationWorkspace Build(DateTimeOffset now)
    {
        CareerApplicationWorkspaceService service = new();
        CareerApplicationService applications = new();
        CareerDocumentBuilderService documents = new();
        CareerMaterialsProof materials = CareerMaterialsProofData.Build(now);
        CvBuilderWorkspace cvWorkspace = CareerDocumentBuilderProofData.Build(now);
        CvBuilderDocument cv = cvWorkspace.Documents.Single(document =>
            document.Id == cvWorkspace.ActiveDocumentId);
        CareerOpportunity opportunity = CareerProofData.Build(now).Opportunities[0] with
        {
            Stage = OpportunityStage.Interested
        };
        CoverLetterDocument letter = service.CreateDraft(
            "demo-cover-letter",
            "Portfolio demo cover letter",
            opportunity,
            cv,
            materials.Facts,
            now);
        foreach (CoverLetterSection section in letter.Sections.Where(section =>
                     section.State == DraftSectionState.Generated))
        {
            letter = service.SetSuggestionState(
                letter,
                section.Id,
                DraftSectionState.Accepted,
                now.AddMinutes(letter.Version));
        }
        letter = service.UpdateSection(
            letter,
            "motivation",
            "I value careful, evidence-backed application delivery and explicit review boundaries.",
            now.AddMinutes(4));

        ApplicationActionResult application = applications.CreateFromApprovedOpportunity(
            opportunity,
            explicitlyApproved: true,
            now);
        CareerApplicationPack[] packs = [];
        if (application.Applied)
        {
            CareerApplicationPack pack = service.CreatePack(
                "demo-application-pack",
                opportunity,
                application.Application,
                cv,
                letter,
                now);
            pack = service.ReviewPack(
                pack,
                cv,
                documents.Review(cv, materials.Facts),
                letter,
                service.Review(letter, opportunity, cv, materials.Facts),
                now);
            packs = [pack];
        }

        return new CareerApplicationWorkspace(
            CareerApplicationWorkspace.CurrentSchemaVersion,
            [opportunity],
            [letter],
            application.Applied ? [application.Application] : [],
            packs,
            materials.Facts,
            opportunity.Id,
            letter.Id);
    }
}
