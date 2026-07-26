namespace LifeOS.Core.CareerStudio;

public sealed class CareerDocumentBuilderService
{
    private static readonly CvSectionKind[] RequiredSections =
    [
        CvSectionKind.Contact,
        CvSectionKind.Profile,
        CvSectionKind.Employment,
        CvSectionKind.Skills
    ];

    public CvBuilderDocument CreateFromCareerProfile(
        string id,
        string name,
        string targetRole,
        IEnumerable<CareerFact> facts,
        DateTimeOffset now)
    {
        CareerFact[] trusted = facts.Where(fact => fact.IsTrusted).ToArray();

        string JoinCategory(string category) =>
            string.Join(
                Environment.NewLine,
                trusted
                    .Where(fact => string.Equals(fact.Category, category, StringComparison.OrdinalIgnoreCase))
                    .Select(fact => fact.FactualValue));

        IReadOnlyList<string> FactIds(string category) =>
            trusted
                .Where(fact => string.Equals(fact.Category, category, StringComparison.OrdinalIgnoreCase))
                .Select(fact => fact.Id)
                .ToArray();

        CvBuilderSection[] sections =
        [
            new("contact", CvSectionKind.Contact, "Contact details",
                "Codie Shannon\nWhakatane, New Zealand\nContact details available on request.",
                0, true, []),
            new("profile", CvSectionKind.Profile, "Professional profile",
                $"Application developer focused on {targetRole} and evidence-backed delivery.",
                1, true, FactIds("Profile")),
            new("employment", CvSectionKind.Employment, "Experience",
                JoinCategory("Experience"),
                2, true, FactIds("Experience")),
            new("skills", CvSectionKind.Skills, "Skills",
                JoinCategory("Skill"),
                3, true, FactIds("Skill")),
            new("projects", CvSectionKind.Projects, "Selected projects",
                JoinCategory("Project"),
                4, true, FactIds("Project")),
            new("education", CvSectionKind.Education, "Education",
                string.Empty,
                5, false, []),
            new("certifications", CvSectionKind.Certifications, "Certifications",
                string.Empty,
                6, false, [])
        ];

        return new CvBuilderDocument(
            id,
            name,
            targetRole,
            "professional",
            sections,
            CvBuilderStep.Content,
            1,
            now,
            true);
    }

    public CvBuilderDocument UpdateSection(
        CvBuilderDocument document,
        string sectionId,
        string heading,
        string content,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(heading))
            throw new ArgumentException("A section heading is required.", nameof(heading));

        if (!document.Sections.Any(section => section.Id == sectionId))
            throw new ArgumentException("The requested CV section does not exist.", nameof(sectionId));

        CvBuilderSection[] sections = document.Sections
            .Select(section => section.Id == sectionId
                ? section with { Heading = heading.Trim(), Content = content.Trim() }
                : section)
            .ToArray();

        return Autosave(document with { Sections = sections }, now);
    }

    public CvBuilderDocument SetSectionEnabled(
        CvBuilderDocument document,
        string sectionId,
        bool isEnabled,
        DateTimeOffset now)
    {
        if (!document.Sections.Any(section => section.Id == sectionId))
            throw new ArgumentException("The requested CV section does not exist.", nameof(sectionId));

        CvBuilderSection[] sections = document.Sections
            .Select(section => section.Id == sectionId
                ? section with { IsEnabled = isEnabled }
                : section)
            .ToArray();

        return Autosave(document with { Sections = sections }, now);
    }

    public CvBuilderDocument MoveSection(
        CvBuilderDocument document,
        string sectionId,
        int direction,
        DateTimeOffset now)
    {
        List<CvBuilderSection> ordered = document.Sections.OrderBy(section => section.Order).ToList();
        int currentIndex = ordered.FindIndex(section => section.Id == sectionId);
        int targetIndex = currentIndex + Math.Sign(direction);

        if (currentIndex < 0)
            throw new ArgumentException("The requested CV section does not exist.", nameof(sectionId));

        if (targetIndex < 0 || targetIndex >= ordered.Count)
            return document;

        (ordered[currentIndex], ordered[targetIndex]) = (ordered[targetIndex], ordered[currentIndex]);

        CvBuilderSection[] sections = ordered
            .Select((section, index) => section with { Order = index })
            .ToArray();

        return Autosave(document with { Sections = sections }, now);
    }

    public CvBuilderDocument SetTargetRole(
        CvBuilderDocument document,
        string targetRole,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(targetRole))
            throw new ArgumentException("A target role is required.", nameof(targetRole));

        return Autosave(document with { TargetRole = targetRole.Trim() }, now);
    }

    public CvBuilderDocument SetStep(
        CvBuilderDocument document,
        CvBuilderStep step,
        DateTimeOffset now) =>
        Autosave(document with { ActiveStep = step }, now);

    public CvBuilderDocument Duplicate(
        CvBuilderDocument document,
        string id,
        string name,
        DateTimeOffset now) =>
        document with
        {
            Id = id,
            Name = name,
            Version = 1,
            UpdatedUtc = now,
            IsAutosaved = true
        };

    public CvBuilderReview Review(
        CvBuilderDocument document,
        IEnumerable<CareerFact> facts)
    {
        Dictionary<string, CareerFact> factMap =
            facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
        List<CvValidationIssue> issues = [];

        foreach (CvSectionKind kind in RequiredSections)
        {
            CvBuilderSection? section = document.Sections.FirstOrDefault(candidate => candidate.Kind == kind);
            if (section is null || !section.IsEnabled || string.IsNullOrWhiteSpace(section.Content))
            {
                issues.Add(new CvValidationIssue(
                    $"required-{kind.ToString().ToLowerInvariant()}",
                    $"{kind} is required before export.",
                    CvValidationSeverity.Blocking,
                    section?.Id));
            }
        }

        IReadOnlyList<string> usedFactIds = document.VisibleSections
            .SelectMany(section => section.SourceFactIds)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        foreach (string factId in usedFactIds)
        {
            if (!factMap.TryGetValue(factId, out CareerFact? fact) || !fact.IsTrusted)
            {
                issues.Add(new CvValidationIssue(
                    "unsupported-source",
                    $"A section references an untrusted or missing career fact: {factId}.",
                    CvValidationSeverity.Blocking));
            }
        }

        if (string.IsNullOrWhiteSpace(document.TargetRole))
        {
            issues.Add(new CvValidationIssue(
                "missing-target-role",
                "Add a target role to guide relevance review.",
                CvValidationSeverity.Warning));
        }

        int completed = RequiredSections.Count(kind =>
            document.Sections.Any(section =>
                section.Kind == kind &&
                section.IsEnabled &&
                !string.IsNullOrWhiteSpace(section.Content)));

        int trusted = usedFactIds.Count(factId =>
            factMap.TryGetValue(factId, out CareerFact? fact) && fact.IsTrusted);

        return new CvBuilderReview(issues, trusted, usedFactIds.Count, completed);
    }

    private static CvBuilderDocument Autosave(CvBuilderDocument document, DateTimeOffset now) =>
        document with
        {
            UpdatedUtc = now,
            IsAutosaved = true,
            Version = document.Version + 1
        };
}

public static class CareerDocumentBuilderProofData
{
    public static CvBuilderWorkspace Build(DateTimeOffset now)
    {
        CareerMaterialsProof materials = CareerMaterialsProofData.Build(now);
        CareerDocumentBuilderService service = new();
        CvBuilderDocument primary = service.CreateFromCareerProfile(
            "cv-software-application",
            "Software Application CV",
            "Software Application Developer",
            materials.Facts,
            now);
        CvBuilderDocument general = service.Duplicate(
            primary,
            "cv-general",
            "General Technology CV",
            now.AddMinutes(1));

        return new CvBuilderWorkspace(
            [primary, general],
            primary.Id,
            service.Review(primary, materials.Facts));
    }
}
