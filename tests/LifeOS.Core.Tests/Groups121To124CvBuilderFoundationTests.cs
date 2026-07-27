using LifeOS.Core.CareerStudio;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups121To124CvBuilderFoundationTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 26, 14, 30, 0, TimeSpan.FromHours(12));

    private readonly CareerMaterialsProof _materials = CareerMaterialsProofData.Build(Now);
    private readonly CareerDocumentBuilderService _service = new();

    [Fact]
    public void AddCustomSectionCreatesEnabledAutosavedSectionWithUniqueId()
    {
        CvBuilderWorkspace workspace = CareerDocumentBuilderProofData.Build(Now);
        CvBuilderDocument original = workspace.Documents.Single(document =>
            document.Id == workspace.ActiveDocumentId);

        CvBuilderDocument first = _service.AddCustomSection(original, "Community work", Now);
        CvBuilderDocument second = _service.AddCustomSection(first, "Awards", Now.AddMinutes(1));

        CvBuilderSection[] custom = second.Sections
            .Where(section => section.Kind == CvSectionKind.Custom)
            .OrderBy(section => section.Order)
            .ToArray();

        Assert.Equal(2, custom.Length);
        Assert.Equal("custom-1", custom[0].Id);
        Assert.Equal("Community work", custom[0].Heading);
        Assert.True(custom[0].IsEnabled);
        Assert.Equal("custom-2", custom[1].Id);
        Assert.Equal("Awards", custom[1].Heading);
        Assert.True(second.IsAutosaved);
        Assert.Equal(original.Version + 2, second.Version);
    }

    [Fact]
    public void CustomSectionRetainsModularSubtitleDatesAndRichContent()
    {
        CvBuilderDocument original = Create();
        CvBuilderDocument withCustom = _service.AddCustomSection(
            original,
            "Community work",
            Now);
        CvBuilderSection custom = withCustom.Sections.Single(section =>
            section.Kind == CvSectionKind.Custom);

        CvBuilderDocument updated = _service.UpdateSectionDetails(
            withCustom,
            custom.Id,
            "Community work",
            "Mentored junior developers.",
            "Volunteer mentor",
            new DateTime(2025, 2, 1),
            new DateTime(2026, 7, 1),
            true,
            "<FlowDocument />",
            true,
            Now.AddMinutes(1));

        CvBuilderSection saved = updated.Sections.Single(section =>
            section.Id == custom.Id);
        Assert.Equal("Volunteer mentor", saved.Subtitle);
        Assert.True(saved.ShowDateRange);
        Assert.Equal(new DateTime(2025, 2, 1), saved.StartDate);
        Assert.Equal(new DateTime(2026, 7, 1), saved.EndDate);
        Assert.Equal("<FlowDocument />", saved.RichContent);
        Assert.True(saved.ShowSubtitle);
    }

    [Fact]
    public void CustomModulesCanBeAddedAndRemovedWithoutLeavingStaleValues()
    {
        CvBuilderDocument withCustom = _service.AddCustomSection(
            Create(),
            "Community work",
            Now);
        CvBuilderSection custom = withCustom.Sections.Single(section =>
            section.Kind == CvSectionKind.Custom);

        CvBuilderDocument enabled = _service.SetSectionModules(
            withCustom,
            custom.Id,
            true,
            true,
            Now.AddMinutes(1));
        Assert.True(enabled.Sections.Single(section => section.Id == custom.Id).ShowSubtitle);
        Assert.True(enabled.Sections.Single(section => section.Id == custom.Id).ShowDateRange);

        CvBuilderDocument removed = _service.SetSectionModules(
            enabled,
            custom.Id,
            false,
            false,
            Now.AddMinutes(2));
        CvBuilderSection saved = removed.Sections.Single(section => section.Id == custom.Id);
        Assert.False(saved.ShowSubtitle);
        Assert.False(saved.ShowDateRange);
        Assert.Null(saved.StartDate);
        Assert.Null(saved.EndDate);
    }

    [Fact]
    public void OptionalSectionCanBeRemovedAndRequiredSectionCannot()
    {
        CvBuilderDocument withCustom = _service.AddCustomSection(
            Create(),
            "Community work",
            Now);
        CvBuilderSection custom = withCustom.Sections.Single(section =>
            section.Kind == CvSectionKind.Custom);

        CvBuilderDocument removed = _service.RemoveSection(
            withCustom,
            custom.Id,
            Now.AddMinutes(1));

        Assert.DoesNotContain(removed.Sections, section => section.Id == custom.Id);
        Assert.Throws<InvalidOperationException>(() =>
            _service.RemoveSection(removed, "profile", Now.AddMinutes(2)));
    }

    [Fact]
    public void EnabledSectionsCanBeReorderedByDragTarget()
    {
        CvBuilderDocument document = Create();

        CvBuilderDocument reordered = _service.MoveSectionBefore(
            document,
            "projects",
            "profile",
            Now);

        string[] visibleIds = reordered.VisibleSections
            .Select(section => section.Id)
            .ToArray();
        Assert.Equal(
            ["contact", "projects", "profile", "employment", "skills"],
            visibleIds);
    }

    [Fact]
    public void DraggingTopSectionOntoBottomSectionPlacesItAfterTarget()
    {
        CvBuilderDocument reordered = _service.MoveSectionAfter(
            Create(),
            "contact",
            "projects",
            Now);

        Assert.Equal(
            ["profile", "employment", "skills", "projects", "contact"],
            reordered.VisibleSections.Select(section => section.Id));
    }

    [Fact]
    public void EmploymentAndEducationUseStructuredRepeatableEntries()
    {
        CvBuilderDocument document = Create();
        CvBuilderSection employment = document.Sections.Single(section =>
            section.Kind == CvSectionKind.Employment);
        Assert.Single(employment.Entries!);

        CvBuilderDocument educationEnabled = _service.SetSectionEnabled(
            document,
            "education",
            true,
            Now);
        CvBuilderDocument withEntry = _service.AddEntry(
            educationEnabled,
            "education",
            Now.AddMinutes(1));
        CvBuilderEntry entry = withEntry.Sections
            .Single(section => section.Id == "education")
            .Entries!
            .Single();

        CvBuilderDocument updated = _service.UpdateEntry(
            withEntry,
            "education",
            entry with
            {
                Title = "Diploma in Software Development",
                Organization = "Example Institute",
                StartDate = new DateTime(2024, 2, 1),
                EndDate = new DateTime(2025, 11, 1),
                Description = "Completed applied software projects."
            },
            Now.AddMinutes(2));

        CvBuilderSection saved = updated.Sections.Single(section =>
            section.Id == "education");
        Assert.Equal("Diploma in Software Development", saved.Entries!.Single().Title);
        Assert.Contains("applied software", saved.Content);
    }

    [Fact]
    public void RemovingStructuredEntryRebuildsSectionContent()
    {
        CvBuilderDocument document = Create();
        CvBuilderSection employment = document.Sections.Single(section =>
            section.Kind == CvSectionKind.Employment);

        CvBuilderDocument removed = _service.RemoveEntry(
            document,
            employment.Id,
            employment.Entries!.Single().Id,
            Now);

        CvBuilderSection saved = removed.Sections.Single(section =>
            section.Id == employment.Id);
        Assert.Empty(saved.Entries!);
        Assert.Empty(saved.Content);
    }

    [Fact]
    public void Creates_guided_cv_from_trusted_career_profile()
    {
        CvBuilderDocument document = Create();

        Assert.Equal("Software Application Developer", document.TargetRole);
        Assert.Equal(CvBuilderStep.Content, document.ActiveStep);
        Assert.Contains(document.Sections, section => section.Kind == CvSectionKind.Projects);
        Assert.DoesNotContain(
            document.Sections.SelectMany(section => section.SourceFactIds),
            id => id == "fact-stale");
    }

    [Fact]
    public void Editing_a_section_autosaves_a_new_version()
    {
        CvBuilderDocument document = Create();

        CvBuilderDocument edited = _service.UpdateSection(
            document,
            "profile",
            "Professional summary",
            "Evidence-backed .NET application developer.",
            Now.AddMinutes(1));

        Assert.True(edited.IsAutosaved);
        Assert.Equal(document.Version + 1, edited.Version);
        Assert.Equal(
            "Evidence-backed .NET application developer.",
            edited.Sections.Single(section => section.Id == "profile").Content);
    }

    [Fact]
    public void Sections_can_be_reordered_without_losing_content()
    {
        CvBuilderDocument document = Create();
        CvBuilderSection projects = document.Sections.Single(section => section.Id == "projects");

        CvBuilderDocument moved = _service.MoveSection(
            document,
            projects.Id,
            -1,
            Now.AddMinutes(1));

        Assert.Equal(projects.Content, moved.Sections.Single(section => section.Id == "projects").Content);
        Assert.True(
            moved.Sections.Single(section => section.Id == "projects").Order <
            moved.Sections.Single(section => section.Id == "skills").Order);
    }

    [Fact]
    public void Required_empty_section_blocks_export()
    {
        CvBuilderDocument document = Create();
        CvBuilderDocument emptyExperience = _service.UpdateSection(
            document,
            "employment",
            "Experience",
            string.Empty,
            Now.AddMinutes(1));

        CvBuilderReview review = _service.Review(emptyExperience, _materials.Facts);

        Assert.False(review.CanExport);
        Assert.Contains(review.Issues, issue => issue.Code == "required-employment");
    }

    [Fact]
    public void Untrusted_source_fact_blocks_export()
    {
        CvBuilderDocument document = Create();
        CvBuilderSection profile = document.Sections.Single(section => section.Id == "profile");
        CvBuilderDocument unsafeDocument = document with
        {
            Sections = document.Sections
                .Select(section => section.Id == profile.Id
                    ? section with { SourceFactIds = ["fact-stale"] }
                    : section)
                .ToArray()
        };

        CvBuilderReview review = _service.Review(unsafeDocument, _materials.Facts);

        Assert.False(review.CanExport);
        Assert.Contains(review.Issues, issue => issue.Code == "unsupported-source");
    }

    [Fact]
    public void Cv_variant_can_be_duplicated_for_another_application()
    {
        CvBuilderDocument copy = _service.Duplicate(
            Create(),
            "cv-new-role",
            "New Role CV",
            Now.AddMinutes(2));

        Assert.Equal("cv-new-role", copy.Id);
        Assert.Equal("New Role CV", copy.Name);
        Assert.Equal(1, copy.Version);
    }

    private CvBuilderDocument Create() =>
        _service.CreateFromCareerProfile(
            "cv-software",
            "Software CV",
            "Software Application Developer",
            _materials.Facts,
            Now);
}
