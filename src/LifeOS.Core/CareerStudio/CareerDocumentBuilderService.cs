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
                2, true, FactIds("Experience"),
                Entries:
                [
                    new(
                        "employment-1",
                        targetRole,
                        "Self-directed and client projects",
                        "Whakatane",
                        new DateTime(2025, 1, 1),
                        null,
                        true,
                        JoinCategory("Experience"))
                ]),
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

    public CvBuilderDocument UpdateSectionDetails(
        CvBuilderDocument document,
        string sectionId,
        string heading,
        string content,
        string subtitle,
        DateTime? startDate,
        DateTime? endDate,
        bool showDateRange,
        string richContent,
        bool showSubtitle,
        DateTimeOffset now)
    {
        if (showDateRange &&
            startDate.HasValue &&
            endDate.HasValue &&
            endDate.Value.Date < startDate.Value.Date)
        {
            throw new ArgumentException("The end date cannot be before the start date.", nameof(endDate));
        }

        CvBuilderDocument updated = UpdateSection(document, sectionId, heading, content, now);
        CvBuilderSection[] sections = updated.Sections
            .Select(section => section.Id == sectionId
                ? section with
                {
                    Subtitle = subtitle.Trim(),
                    StartDate = startDate,
                    EndDate = endDate,
                    ShowDateRange = showDateRange,
                    RichContent = richContent,
                    ShowSubtitle = showSubtitle
                }
                : section)
            .ToArray();

        return updated with { Sections = sections };
    }

    public CvBuilderDocument SetSectionModules(
        CvBuilderDocument document,
        string sectionId,
        bool showSubtitle,
        bool showDateRange,
        DateTimeOffset now)
    {
        if (!document.Sections.Any(section => section.Id == sectionId))
            throw new ArgumentException("The requested CV section does not exist.", nameof(sectionId));

        CvBuilderSection[] sections = document.Sections
            .Select(section => section.Id == sectionId
                ? section with
                {
                    ShowSubtitle = showSubtitle,
                    ShowDateRange = showDateRange,
                    Subtitle = showSubtitle ? section.Subtitle : string.Empty,
                    StartDate = showDateRange ? section.StartDate : null,
                    EndDate = showDateRange ? section.EndDate : null
                }
                : section)
            .ToArray();

        return Autosave(document with { Sections = sections }, now);
    }

    public CvBuilderDocument RenameDocument(
        CvBuilderDocument document,
        string name,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A document name is required.", nameof(name));

        return Autosave(document with { Name = name.Trim() }, now);
    }

    public CvBuilderDocument AddCustomSection(
        CvBuilderDocument document,
        string heading,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(heading))
            throw new ArgumentException("A custom section heading is required.", nameof(heading));

        int suffix = 1;
        string sectionId;
        do
        {
            sectionId = $"custom-{suffix++}";
        }
        while (document.Sections.Any(section => section.Id == sectionId));

        CvBuilderSection section = new(
            sectionId,
            CvSectionKind.Custom,
            heading.Trim(),
            string.Empty,
            document.Sections.Count,
            true,
            []);

        return Autosave(
            document with { Sections = [.. document.Sections, section] },
            now);
    }

    public CvBuilderDocument RemoveSection(
        CvBuilderDocument document,
        string sectionId,
        DateTimeOffset now)
    {
        CvBuilderSection? target = document.Sections.FirstOrDefault(section =>
            section.Id == sectionId);
        if (target is null)
            throw new ArgumentException("The requested CV section does not exist.", nameof(sectionId));
        if (RequiredSections.Contains(target.Kind))
            throw new InvalidOperationException("Required CV sections cannot be removed.");

        CvBuilderSection[] sections = document.Sections
            .Where(section => section.Id != sectionId)
            .OrderBy(section => section.Order)
            .Select((section, index) => section with { Order = index })
            .ToArray();

        return Autosave(document with { Sections = sections }, now);
    }

    public CvBuilderDocument AddEntry(
        CvBuilderDocument document,
        string sectionId,
        DateTimeOffset now)
    {
        CvBuilderSection section = document.Sections.SingleOrDefault(candidate =>
            candidate.Id == sectionId)
            ?? throw new ArgumentException("The requested CV section does not exist.", nameof(sectionId));
        if (section.Kind is not (CvSectionKind.Employment or CvSectionKind.Education))
            throw new InvalidOperationException("Only employment and education sections use structured entries.");

        List<CvBuilderEntry> entries = section.Entries?.ToList() ?? [];
        int suffix = 1;
        string prefix = section.Kind == CvSectionKind.Employment ? "employment" : "education";
        string entryId;
        do
        {
            entryId = $"{prefix}-{suffix++}";
        }
        while (entries.Any(entry => entry.Id == entryId));

        entries.Add(new CvBuilderEntry(
            entryId,
            string.Empty,
            string.Empty,
            string.Empty,
            null,
            null,
            false,
            string.Empty));

        CvBuilderSection[] sections = document.Sections
            .Select(candidate => candidate.Id == sectionId
                ? candidate with { Entries = entries }
                : candidate)
            .ToArray();
        return Autosave(document with { Sections = sections }, now);
    }

    public CvBuilderDocument UpdateEntry(
        CvBuilderDocument document,
        string sectionId,
        CvBuilderEntry replacement,
        DateTimeOffset now)
    {
        CvBuilderSection section = document.Sections.SingleOrDefault(candidate =>
            candidate.Id == sectionId)
            ?? throw new ArgumentException("The requested CV section does not exist.", nameof(sectionId));
        if (section.Entries is null ||
            section.Entries.All(entry => entry.Id != replacement.Id))
        {
            throw new ArgumentException("The requested structured entry does not exist.", nameof(replacement));
        }

        CvBuilderEntry[] entries = section.Entries
            .Select(entry => entry.Id == replacement.Id ? replacement : entry)
            .ToArray();
        string combinedContent = string.Join(
            Environment.NewLine,
            entries
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Description))
                .Select(entry => entry.Description.Trim()));
        CvBuilderSection[] sections = document.Sections
            .Select(candidate => candidate.Id == sectionId
                ? candidate with { Entries = entries, Content = combinedContent }
                : candidate)
            .ToArray();
        return Autosave(document with { Sections = sections }, now);
    }

    public CvBuilderDocument RemoveEntry(
        CvBuilderDocument document,
        string sectionId,
        string entryId,
        DateTimeOffset now)
    {
        CvBuilderSection section = document.Sections.SingleOrDefault(candidate =>
            candidate.Id == sectionId)
            ?? throw new ArgumentException("The requested CV section does not exist.", nameof(sectionId));
        CvBuilderEntry[] entries = (section.Entries ?? [])
            .Where(entry => entry.Id != entryId)
            .ToArray();
        if ((section.Entries?.Count ?? 0) == entries.Length)
            throw new ArgumentException("The requested structured entry does not exist.", nameof(entryId));

        string combinedContent = string.Join(
            Environment.NewLine,
            entries
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Description))
                .Select(entry => entry.Description.Trim()));
        CvBuilderSection[] sections = document.Sections
            .Select(candidate => candidate.Id == sectionId
                ? candidate with { Entries = entries, Content = combinedContent }
                : candidate)
            .ToArray();
        return Autosave(document with { Sections = sections }, now);
    }

    public CvBuilderDocument MoveSection(
        CvBuilderDocument document,
        string sectionId,
        int direction,
        DateTimeOffset now)
    {
        List<CvBuilderSection> ordered = document.Sections
            .Where(section => section.IsEnabled)
            .OrderBy(section => section.Order)
            .ToList();
        int currentIndex = ordered.FindIndex(section => section.Id == sectionId);
        int targetIndex = currentIndex + Math.Sign(direction);

        if (currentIndex < 0)
            throw new ArgumentException("The requested CV section does not exist.", nameof(sectionId));

        if (targetIndex < 0 || targetIndex >= ordered.Count)
            return document;

        (ordered[currentIndex], ordered[targetIndex]) = (ordered[targetIndex], ordered[currentIndex]);

        IEnumerable<CvBuilderSection> disabled = document.Sections
            .Where(section => !section.IsEnabled)
            .OrderBy(section => section.Order);
        CvBuilderSection[] sections = ordered
            .Concat(disabled)
            .Select((section, index) => section with { Order = index })
            .ToArray();

        return Autosave(document with { Sections = sections }, now);
    }

    public CvBuilderDocument MoveSectionBefore(
        CvBuilderDocument document,
        string sectionId,
        string targetSectionId,
        DateTimeOffset now) =>
        MoveSectionRelative(document, sectionId, targetSectionId, false, now);

    public CvBuilderDocument MoveSectionAfter(
        CvBuilderDocument document,
        string sectionId,
        string targetSectionId,
        DateTimeOffset now) =>
        MoveSectionRelative(document, sectionId, targetSectionId, true, now);

    private static CvBuilderDocument MoveSectionRelative(
        CvBuilderDocument document,
        string sectionId,
        string targetSectionId,
        bool insertAfter,
        DateTimeOffset now)
    {
        if (sectionId == targetSectionId)
            return document;

        List<CvBuilderSection> enabled = document.Sections
            .Where(section => section.IsEnabled)
            .OrderBy(section => section.Order)
            .ToList();
        CvBuilderSection? moving = enabled.FirstOrDefault(section =>
            section.Id == sectionId);
        if (moving is null ||
            enabled.All(section => section.Id != targetSectionId))
        {
            throw new ArgumentException("Both sections must exist and be enabled.");
        }

        enabled.Remove(moving);
        int targetIndex = enabled.FindIndex(section =>
            section.Id == targetSectionId);
        if (insertAfter)
            targetIndex++;
        enabled.Insert(targetIndex, moving);

        IEnumerable<CvBuilderSection> disabled = document.Sections
            .Where(section => !section.IsEnabled)
            .OrderBy(section => section.Order);
        CvBuilderSection[] sections = enabled
            .Concat(disabled)
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
