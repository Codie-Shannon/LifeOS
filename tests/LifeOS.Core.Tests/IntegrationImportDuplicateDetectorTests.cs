using LifeOS.Core.IntegrationConnectors;
using LifeOS.Core.IntegrationInbox;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class IntegrationImportDuplicateDetectorTests
{
    [Fact]
    public void ImportMarksDuplicateWhenKeyAlreadyExists()
    {
        var existing = new[]
        {
            new IntegrationPreviewItem
            {
                DuplicateKey = "manual-csv:bill-1:20260710:91.45",
                Status = IntegrationPreviewStatus.New
            }
        };
        var incoming = new[]
        {
            new IntegrationPreviewItem
            {
                DuplicateKey = "manual-csv:bill-1:20260710:91.45",
                Status = IntegrationPreviewStatus.New
            }
        };

        var duplicateCount = IntegrationImportDuplicateDetector.MarkDuplicateSuspicions(existing, incoming);

        Assert.Equal(1, duplicateCount);
        Assert.Equal(IntegrationPreviewStatus.DuplicateSuspected, incoming[0].Status);
        Assert.Contains("already exists", incoming[0].ReviewNote);
    }

    [Fact]
    public void ImportMarksDuplicateWithinIncomingBatch()
    {
        var incoming = new[]
        {
            new IntegrationPreviewItem
            {
                DuplicateKey = "manual-csv:bill-1:20260710:91.45",
                Status = IntegrationPreviewStatus.New
            },
            new IntegrationPreviewItem
            {
                DuplicateKey = "manual-csv:bill-1:20260710:91.45",
                Status = IntegrationPreviewStatus.New
            }
        };

        var duplicateCount = IntegrationImportDuplicateDetector.MarkDuplicateSuspicions([], incoming);

        Assert.Equal(1, duplicateCount);
        Assert.Equal(IntegrationPreviewStatus.New, incoming[0].Status);
        Assert.Equal(IntegrationPreviewStatus.DuplicateSuspected, incoming[1].Status);
        Assert.Contains("more than once", incoming[1].ReviewNote);
    }

    [Fact]
    public void DuplicateSuspectedPreviewStillCannotBeAccepted()
    {
        var incoming = new[]
        {
            new IntegrationPreviewItem
            {
                DuplicateKey = "manual-csv:bill-1:20260710:91.45",
                Status = IntegrationPreviewStatus.New,
                TrustState = IntegrationTrustState.SourceBacked,
                SourceEvidence = "sample.csv#1"
            }
        };

        IntegrationImportDuplicateDetector.MarkDuplicateSuspicions(
            [new IntegrationPreviewItem { DuplicateKey = "manual-csv:bill-1:20260710:91.45" }],
            incoming);

        Assert.Throws<InvalidOperationException>(() =>
            IntegrationInboxReviewEngine.Accept(incoming[0], "Looks right."));
    }
}
