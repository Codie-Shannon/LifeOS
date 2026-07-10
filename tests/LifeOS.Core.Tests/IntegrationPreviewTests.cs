using LifeOS.Core.IntegrationInbox;
using LifeOS.Shared.IntegrationInbox;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class IntegrationPreviewTests
{
    [Fact]
    public void IntakeCreatesReviewPreview()
    {
        var preview = IntegrationPreviewIntake.CreatePreview(new IntegrationPreviewDraft
        {
            SourceKind = IntegrationSourceKind.ManualImport,
            SourceLabel = "manual-json",
            ExternalReference = "row-42",
            Title = "Imported bill candidate",
            Summary = "Possible bill from manual import.",
            Amount = 72.5m,
            Currency = "nzd",
            SuggestedTarget = IntegrationTargetKind.BillsPayments,
            SourceEvidence = "manual-import.json"
        });

        Assert.Equal(IntegrationPreviewStatus.New, preview.Status);
        Assert.Equal(IntegrationTrustState.Untrusted, preview.TrustState);
        Assert.True(preview.IsReadOnlyPreview);
        Assert.True(preview.RequiresHumanReview);
        Assert.Equal("NZD", preview.Currency);
        Assert.False(string.IsNullOrWhiteSpace(preview.DuplicateKey));
    }

    [Fact]
    public void IntakeRequiresProvenanceIdentity()
    {
        Assert.Throws<ArgumentException>(() => IntegrationPreviewIntake.CreatePreview(new IntegrationPreviewDraft
        {
            SourceKind = IntegrationSourceKind.Email,
            ExternalReference = "message-1",
            Title = "Missing source label"
        }));

        Assert.Throws<ArgumentException>(() => IntegrationPreviewIntake.CreatePreview(new IntegrationPreviewDraft
        {
            SourceKind = IntegrationSourceKind.Email,
            SourceLabel = "gmail",
            Title = "Missing external reference"
        }));
    }

    [Fact]
    public void AcceptBlocksDuplicateSuspected()
    {
        var item = ReviewablePreview();
        item.Status = IntegrationPreviewStatus.DuplicateSuspected;

        Assert.Throws<InvalidOperationException>(() => IntegrationInboxReviewEngine.Accept(item, "Looks okay."));
        Assert.Equal(IntegrationPreviewStatus.DuplicateSuspected, item.Status);
    }

    [Fact]
    public void AcceptRequiresSourceBackedTrust()
    {
        var item = ReviewablePreview();
        item.TrustState = IntegrationTrustState.Untrusted;

        Assert.Throws<InvalidOperationException>(() => IntegrationInboxReviewEngine.Accept(item, "Looks okay."));
        Assert.Equal(IntegrationPreviewStatus.New, item.Status);
    }

    [Fact]
    public void LinkRequiresAcceptedPreview()
    {
        var item = ReviewablePreview();

        Assert.Throws<InvalidOperationException>(() => IntegrationInboxReviewEngine.Link(item, "target-1"));

        IntegrationInboxReviewEngine.Accept(item, "Accepted for handoff.");
        IntegrationInboxReviewEngine.Link(item, "target-1");

        Assert.Equal(IntegrationPreviewStatus.Linked, item.Status);
        Assert.Equal(IntegrationTrustState.Trusted, item.TrustState);
        Assert.Equal("target-1", item.LinkReference);
    }

    [Fact]
    public void SummaryCountsReviewAndPreviewMoney()
    {
        var summary = IntegrationInboxCalculator.Calculate(
        [
            new IntegrationPreviewItem
            {
                SourceKind = IntegrationSourceKind.Accounting,
                Status = IntegrationPreviewStatus.New,
                TrustState = IntegrationTrustState.Untrusted,
                Amount = 100m
            },
            new IntegrationPreviewItem
            {
                SourceKind = IntegrationSourceKind.Calendar,
                Status = IntegrationPreviewStatus.Accepted,
                TrustState = IntegrationTrustState.Reviewed,
                Amount = 50m
            },
            new IntegrationPreviewItem
            {
                SourceKind = IntegrationSourceKind.Email,
                Status = IntegrationPreviewStatus.DuplicateSuspected,
                TrustState = IntegrationTrustState.SourceBacked
            }
        ]);

        Assert.Equal(3, summary.Total);
        Assert.Equal(2, summary.NeedsReview);
        Assert.Equal(1, summary.ReadyForHandoff);
        Assert.Equal(150m, summary.PreviewMoney);
        Assert.Equal("High", summary.PressureLabel);
    }

    [Fact]
    public void DemoDataRespectsPreviewContract()
    {
        var demo = IntegrationInboxDemoData.Create();

        Assert.NotEmpty(demo);

        foreach (var item in demo)
        {
            Assert.True(item.IsReadOnlyPreview);
            Assert.True(item.RequiresHumanReview);
            Assert.False(string.IsNullOrWhiteSpace(item.SourceLabel));
            Assert.False(string.IsNullOrWhiteSpace(item.ExternalReference));
            Assert.False(string.IsNullOrWhiteSpace(item.DuplicateKey));
        }
    }

    private static IntegrationPreviewItem ReviewablePreview()
    {
        return new IntegrationPreviewItem
        {
            SourceKind = IntegrationSourceKind.ManualImport,
            SourceLabel = "manual-json",
            ExternalReference = "row-42",
            Title = "Imported candidate",
            Status = IntegrationPreviewStatus.New,
            TrustState = IntegrationTrustState.SourceBacked,
            SourceEvidence = "manual-import.json",
            DuplicateKey = "manual-json:row-42"
        };
    }
}
