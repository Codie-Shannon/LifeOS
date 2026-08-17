using LifeOS.Core.CareerStudio;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups129To132CareerApplicationIntegrationTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 18, 20, 0, 0, TimeSpan.FromHours(12));

    private readonly CareerApplicationWorkspaceService _service = new();
    private readonly CareerApplicationService _applications = new();
    private readonly CareerDocumentBuilderService _documents = new();
    private readonly CareerMaterialsProof _materials = CareerMaterialsProofData.Build(Now);

    [Fact]
    public void Manual_opportunity_is_real_empty_state_input_not_proof_seed()
    {
        CareerOpportunity opportunity = Opportunity();

        Assert.Equal(OpportunitySourceType.ManualCapture, opportunity.Source.Type);
        Assert.Equal(OpportunityStage.Reviewing, opportunity.Stage);
        Assert.Empty(opportunity.Requirements);
        Assert.Contains("captured manually", opportunity.History.Single().SafeSummary);
    }

    [Fact]
    public void Draft_links_opportunity_cv_and_only_trusted_provenance()
    {
        CareerOpportunity opportunity = Opportunity();
        CvBuilderDocument cv = Cv();

        CoverLetterDocument letter = _service.CreateDraft(
            "letter-1",
            "Developer cover letter",
            opportunity,
            cv,
            _materials.Facts,
            Now);

        Assert.Equal(opportunity.Id, letter.OpportunityId);
        Assert.Equal(cv.Id, letter.CvDocumentId);
        Assert.DoesNotContain("fact-stale", letter.SourceFactIds);
        Assert.All(
            letter.SourceFactIds,
            id => Assert.True(_materials.Facts.Single(fact => fact.Id == id).IsTrusted));
    }

    [Fact]
    public void Generated_suggestions_require_explicit_acceptance()
    {
        var data = Draft();

        CoverLetterReview before = _service.Review(
            data.letter,
            data.opportunity,
            data.cv,
            _materials.Facts);
        CoverLetterDocument accepted = AcceptGenerated(data.letter);
        accepted = _service.UpdateSection(
            accepted,
            "motivation",
            "I value careful, evidence-backed application delivery.",
            Now.AddMinutes(3));
        CoverLetterReview after = _service.Review(
            accepted,
            data.opportunity,
            data.cv,
            _materials.Facts);

        Assert.False(before.CanExport);
        Assert.Contains(before.Issues, issue => issue.Code == "unreviewed-suggestion");
        Assert.True(after.CanExport);
        Assert.Equal(2, after.AcceptedSuggestionCount);
    }

    [Fact]
    public void Contact_details_are_per_document_and_require_confirmation()
    {
        var data = ReadyDraft();
        CoverLetterDocument unconfirmed = _service.SetContactDetails(
            data.letter,
            include: true,
            confirmed: false,
            Now.AddMinutes(4));
        CoverLetterReview blocked = _service.Review(
            unconfirmed,
            data.opportunity,
            data.cv,
            _materials.Facts);

        CoverLetterDocument confirmed = _service.SetContactDetails(
            unconfirmed,
            include: true,
            confirmed: true,
            Now.AddMinutes(5));
        CoverLetterReview ready = _service.Review(
            confirmed,
            data.opportunity,
            data.cv,
            _materials.Facts);

        Assert.Contains(blocked.Issues, issue => issue.Code == "contact-confirmation");
        Assert.True(ready.CanExport);
    }

    [Theory]
    [InlineData(CvExportFormat.Pdf, ".pdf")]
    [InlineData(CvExportFormat.Docx, ".docx")]
    public void Reviewed_letter_exports_as_versioned_derivative(
        CvExportFormat format,
        string extension)
    {
        var data = ReadyDraft();
        CoverLetterReview review = _service.Review(
            data.letter,
            data.opportunity,
            data.cv,
            _materials.Facts);

        CvExportArtifact artifact = _service.Export(
            data.letter,
            review,
            data.opportunity,
            format,
            Now.AddMinutes(5));

        Assert.EndsWith(extension, artifact.SuggestedFileName);
        Assert.Equal(data.letter.Version, artifact.SourceVersion);
        Assert.NotEmpty(artifact.Content);
    }

    [Fact]
    public void Application_pack_requires_same_opportunity_and_explicit_review()
    {
        var data = ReadyDraft();
        CareerApplication application = Application(data.opportunity);
        CareerApplicationPack pack = _service.CreatePack(
            "pack-1",
            data.opportunity,
            application,
            data.cv,
            data.letter,
            Now.AddMinutes(5));
        CoverLetterReview review = _service.Review(
            data.letter,
            data.opportunity,
            data.cv,
            _materials.Facts);

        Assert.False(pack.IsReady);

        CareerApplicationPack approved = _service.ReviewPack(
            pack,
            data.cv,
            _documents.Review(data.cv, _materials.Facts),
            data.letter,
            review,
            Now.AddMinutes(6));

        Assert.True(approved.IsReady);
        Assert.Equal(2, approved.Documents.Count);
    }

    [Fact]
    public void Later_document_version_marks_reviewed_pack_stale()
    {
        var data = ReadyDraft();
        CareerApplicationPack pack = _service.CreatePack(
            "pack-1",
            data.opportunity,
            Application(data.opportunity),
            data.cv,
            data.letter,
            Now);
        CoverLetterReview review = _service.Review(
            data.letter,
            data.opportunity,
            data.cv,
            _materials.Facts);
        pack = _service.ReviewPack(
            pack,
            data.cv,
            _documents.Review(data.cv, _materials.Facts),
            data.letter,
            review,
            Now);
        CoverLetterDocument edited = _service.UpdateSection(
            data.letter,
            "closing",
            "Thank you. I welcome the opportunity to discuss the role.",
            Now.AddMinutes(1));

        CareerApplicationPack refreshed = _service.RefreshFreshness(
            pack,
            data.cv,
            edited,
            Now.AddMinutes(1));

        Assert.False(refreshed.IsReady);
        Assert.False(refreshed.Reviewed);
        Assert.Contains(
            refreshed.Documents,
            link => link.Kind == CareerDocumentKind.CoverLetter &&
                    link.Freshness == MaterialFreshnessState.Stale);
    }

    [Fact]
    public void Workspace_survives_restart_and_keeps_links()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            $"lifeos-career-{Guid.NewGuid():N}");
        string path = Path.Combine(directory, "workspace.json");
        try
        {
            var data = ReadyDraft();
            CareerApplication application = Application(data.opportunity);
            CareerApplicationPack pack = _service.CreatePack(
                "pack-1",
                data.opportunity,
                application,
                data.cv,
                data.letter,
                Now);
            CareerApplicationWorkspace expected = new(
                CareerApplicationWorkspace.CurrentSchemaVersion,
                [data.opportunity],
                [data.letter],
                [application],
                [pack],
                _materials.Facts,
                data.opportunity.Id,
                data.letter.Id);

            CareerApplicationWorkspaceStore.Save(expected, path);
            CareerApplicationWorkspace actual =
                CareerApplicationWorkspaceStore.Load(path);

            Assert.Equal(expected.ActiveOpportunityId, actual.ActiveOpportunityId);
            Assert.Equal(expected.ActiveCoverLetterId, actual.ActiveCoverLetterId);
            Assert.Equal(data.cv.Id, actual.CoverLetters.Single().CvDocumentId);
            Assert.Equal(application.Id, actual.Packs.Single().ApplicationId);
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Corrupt_workspace_is_preserved_and_backup_recovers()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            $"lifeos-career-{Guid.NewGuid():N}");
        string path = Path.Combine(directory, "workspace.json");
        try
        {
            var data = ReadyDraft();
            CareerApplicationWorkspace workspace = new(
                CareerApplicationWorkspace.CurrentSchemaVersion,
                [data.opportunity],
                [data.letter],
                [],
                [],
                _materials.Facts,
                data.opportunity.Id,
                data.letter.Id);
            CareerApplicationWorkspaceStore.Save(workspace, path);
            CareerApplicationWorkspaceStore.Save(workspace, path);
            File.WriteAllText(path, "{not-json");

            CareerApplicationWorkspace recovered =
                CareerApplicationWorkspaceStore.Load(path);

            Assert.Single(recovered.Opportunities);
            Assert.Single(Directory.GetFiles(directory, "*.unreadable-*"));
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Workspace_drops_orphaned_letter_and_clears_its_active_link()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            $"lifeos-career-{Guid.NewGuid():N}");
        string path = Path.Combine(directory, "workspace.json");
        try
        {
            var data = Draft();
            CareerApplicationWorkspace workspace = new(
                CareerApplicationWorkspace.CurrentSchemaVersion,
                [],
                [data.letter],
                [],
                [],
                [],
                string.Empty,
                data.letter.Id);

            CareerApplicationWorkspaceStore.Save(workspace, path);
            CareerApplicationWorkspace actual =
                CareerApplicationWorkspaceStore.Load(path);

            Assert.Empty(actual.CoverLetters);
            Assert.Empty(actual.ActiveCoverLetterId);
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    private CareerOpportunity Opportunity() => _service.CreateManualOpportunity(
        "opp-1",
        "Software Application Developer",
        "Example Employer",
        "Build and maintain evidence-backed applications.",
        Now);

    private CvBuilderDocument Cv() => _documents.CreateFromCareerProfile(
        "cv-1",
        "Software application CV",
        "Software Application Developer",
        _materials.Facts,
        Now);

    private (CareerOpportunity opportunity, CvBuilderDocument cv, CoverLetterDocument letter) Draft()
    {
        CareerOpportunity opportunity = Opportunity();
        CvBuilderDocument cv = Cv();
        return (
            opportunity,
            cv,
            _service.CreateDraft(
                "letter-1",
                "Developer cover letter",
                opportunity,
                cv,
                _materials.Facts,
                Now));
    }

    private (CareerOpportunity opportunity, CvBuilderDocument cv, CoverLetterDocument letter) ReadyDraft()
    {
        var data = Draft();
        CoverLetterDocument letter = AcceptGenerated(data.letter);
        letter = _service.UpdateSection(
            letter,
            "motivation",
            "I value careful, evidence-backed application delivery.",
            Now.AddMinutes(3));
        return (data.opportunity, data.cv, letter);
    }

    private CoverLetterDocument AcceptGenerated(CoverLetterDocument letter)
    {
        foreach (CoverLetterSection section in letter.Sections.Where(section =>
                     section.State == DraftSectionState.Generated))
        {
            letter = _service.SetSuggestionState(
                letter,
                section.Id,
                DraftSectionState.Accepted,
                Now.AddMinutes(letter.Version));
        }
        return letter;
    }

    private CareerApplication Application(CareerOpportunity opportunity)
    {
        CareerOpportunity approved = opportunity with
        {
            Stage = OpportunityStage.Interested
        };
        ApplicationActionResult result = _applications.CreateFromApprovedOpportunity(
            approved,
            explicitlyApproved: true,
            Now);
        Assert.True(result.Applied);
        return result.Application;
    }
}
