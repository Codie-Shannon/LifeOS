using System.IO.Compression;
using LifeOS.Core.CareerStudio;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups125To128CvTemplatesExportTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 27, 13, 30, 0, TimeSpan.FromHours(12));

    private readonly CareerDocumentLayoutService _layouts = new();
    private readonly CareerDocumentBuilderService _builder = new();
    private readonly CareerMaterialsProof _materials = CareerMaterialsProofData.Build(Now);

    [Fact]
    public void Template_catalog_provides_distinct_ATS_safe_layouts()
    {
        IReadOnlyList<CvTemplateDefinition> templates = _layouts.GetTemplates();

        Assert.Equal(4, templates.Count);
        Assert.All(templates, template => Assert.True(template.AtsFriendly));
        Assert.Equal(4, templates.Select(template => template.Layout.AccentHex).Distinct().Count());
    }

    [Fact]
    public void Applying_template_autosaves_layout_and_increments_version()
    {
        CvBuilderDocument document = Create();

        CvBuilderDocument updated = _layouts.ApplyTemplate(document, "modern", Now);

        Assert.Equal("modern", updated.TemplateId);
        Assert.Equal("#6C4EE3", updated.EffectiveLayout.AccentHex);
        Assert.Equal(document.Version + 1, updated.Version);
        Assert.True(updated.IsAutosaved);
    }

    [Fact]
    public void Layout_controls_enforce_readable_A4_boundaries()
    {
        CvBuilderDocument document = Create();
        CvDocumentLayout valid = document.EffectiveLayout with
        {
            FontScale = 1.1,
            PageMarginMillimetres = 20,
            AccentHex = "#176B65"
        };

        CvBuilderDocument updated = _layouts.UpdateLayout(document, valid, Now);

        Assert.Equal(1.1, updated.EffectiveLayout.FontScale);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _layouts.UpdateLayout(document, valid with { FontScale = 0.7 }, Now));
        Assert.Throws<ArgumentException>(() =>
            _layouts.UpdateLayout(document, valid with { AccentHex = "purple" }, Now));
    }

    [Fact]
    public void Readability_review_reports_ATS_checks_score_and_page_estimate()
    {
        CvReadabilityReview review = _layouts.Review(Create());

        Assert.InRange(review.Score, 75, 100);
        Assert.InRange(review.EstimatedPages, 1, 3);
        Assert.Contains(review.Checks, check => check.Code == "ats-template" && check.Passed);
        Assert.True(review.CanExport);
    }

    [Fact]
    public void Version_snapshot_retains_layout_and_enabled_section_summary()
    {
        CvBuilderDocument document = _layouts.ApplyTemplate(Create(), "compact", Now);

        CvVersionSnapshot snapshot = _layouts.CreateSnapshot(document, "Compact review");

        Assert.Equal(document.Version, snapshot.Version);
        Assert.Equal("compact", snapshot.TemplateId);
        Assert.Equal(document.VisibleSections.Count, snapshot.EnabledSectionCount);
    }

    [Fact]
    public void Pdf_export_is_a_valid_versioned_derivative()
    {
        CvBuilderDocument document = Create();
        CvBuilderReview sourceReview = _builder.Review(document, _materials.Facts);

        CvExportArtifact artifact = _layouts.Export(
            document,
            sourceReview,
            CvExportFormat.Pdf,
            Now);

        Assert.EndsWith($"-v{document.Version}.pdf", artifact.SuggestedFileName);
        Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(artifact.Content, 0, 5));
        Assert.Equal(document.Version, artifact.SourceVersion);
    }

    [Fact]
    public void Docx_export_contains_openxml_document_and_A4_page_settings()
    {
        CvBuilderDocument document = Create();
        CvBuilderReview sourceReview = _builder.Review(document, _materials.Facts);
        CvExportArtifact artifact = _layouts.Export(
            document,
            sourceReview,
            CvExportFormat.Docx,
            Now);

        using MemoryStream stream = new(artifact.Content);
        using ZipArchive archive = new(stream, ZipArchiveMode.Read);
        ZipArchiveEntry documentXml = Assert.Single(
            archive.Entries,
            entry => entry.FullName == "word/document.xml");
        using StreamReader reader = new(documentXml.Open());
        string xml = reader.ReadToEnd();

        Assert.Contains("11906", xml);
        Assert.Contains("16838", xml);
        Assert.EndsWith(".docx", artifact.SuggestedFileName);
    }

    [Fact]
    public void Export_fails_closed_when_source_validation_blocks_document()
    {
        CvBuilderDocument document = Create();
        CvBuilderSection profile = document.Sections.Single(section =>
            section.Kind == CvSectionKind.Profile);
        CvBuilderDocument unsafeDocument = document with
        {
            Sections = document.Sections
                .Select(section => section.Id == profile.Id
                    ? section with { SourceFactIds = ["missing-fact"] }
                    : section)
                .ToArray()
        };
        CvBuilderReview review = _builder.Review(unsafeDocument, _materials.Facts);

        Assert.Throws<InvalidOperationException>(() =>
            _layouts.Export(unsafeDocument, review, CvExportFormat.Pdf, Now));
    }

    private CvBuilderDocument Create() =>
        _builder.CreateFromCareerProfile(
            "cv-sg82",
            "Software Application CV",
            "Software Application Developer",
            _materials.Facts,
            Now);
}
