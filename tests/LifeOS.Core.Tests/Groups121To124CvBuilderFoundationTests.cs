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
