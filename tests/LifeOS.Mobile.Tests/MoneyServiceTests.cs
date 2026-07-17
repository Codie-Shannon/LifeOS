using System.Globalization;
using LifeOS.Mobile.Core.Money;
using Xunit;

namespace LifeOS.Mobile.Tests;

public sealed class MoneyServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 17, 10, 0, 0, TimeSpan.FromHours(12));

    [Fact]
    public void SummaryUsesTrustedSnapshotValues()
    {
        var snapshot = new MoneyService().BuildSnapshot(Now);

        Assert.Equal("NZD", snapshot.Summary.Currency);
        Assert.Equal(830m, snapshot.Summary.Income);
        Assert.Equal(42.50m, snapshot.Summary.Expenses);
    }

    [Fact]
    public void InvoiceStatusesRemainExplicit()
    {
        var snapshot = new MoneyService().BuildSnapshot(Now);

        Assert.Contains(
            snapshot.Invoices,
            x => x.Status == MoneyRecordStatus.Due);

        Assert.Contains(
            snapshot.Invoices,
            x => x.Status == MoneyRecordStatus.Received);
    }

    [Fact]
    public void EvidenceDraftDoesNotDeleteOriginal()
    {
        var draft = new MoneyService().CreateEvidenceDraft(
            "receipt.jpg",
            1200,
            "Software",
            "Safe preview",
            Now);

        Assert.Equal(EvidenceReviewState.Draft, draft.ReviewState);
        Assert.Equal("receipt.jpg", draft.FileName);
    }

    [Fact]
    public void QueueEvidenceIsExplicit()
    {
        var service = new MoneyService();
        var draft = service.CreateEvidenceDraft(
            "receipt.jpg",
            1200,
            "Software",
            "Safe preview",
            Now);

        var queued = service.QueueEvidence(draft);

        Assert.Equal(EvidenceReviewState.Queued, queued.ReviewState);
    }

    [Fact]
    public void DuplicateCandidateUsesExactHash()
    {
        var snapshot = new MoneyService().BuildSnapshot(Now);
        var service = new MoneyService();

        Assert.True(
            service.IsDuplicateCandidate(
                snapshot.Evidence[0],
                snapshot.Evidence[1]));
    }

    [Fact]
    public void EvidenceLinkingRequiresExplicitRecord()
    {
        var service = new MoneyService();
        var draft = service.CreateEvidenceDraft(
            "receipt.jpg",
            1200,
            "Other",
            "Safe preview",
            Now);

        var linked = service.LinkEvidence(
            draft,
            "txn-1",
            "Software");

        Assert.Equal(EvidenceReviewState.Linked, linked.ReviewState);
        Assert.Equal("txn-1", linked.LinkedRecordId);
        Assert.Equal("Software", linked.Category);
    }

    [Fact]
    public void AmountParsingIsLocaleAware()
    {
        var amount = MoneyService.ParseAmount(
            "42.50",
            CultureInfo.GetCultureInfo("en-NZ"));

        Assert.Equal(42.50m, amount);
    }

    [Fact]
    public void InvalidAmountIsRejected()
    {
        Assert.Throws<FormatException>(
            () => MoneyService.ParseAmount(
                "-1",
                CultureInfo.InvariantCulture));
    }

    [Fact]
    public void NoInvoiceContainsPaymentInitiationCapability()
    {
        var snapshot = new MoneyService().BuildSnapshot(Now);

        Assert.DoesNotContain(
            snapshot.Invoices,
            x => x.LinkedEvidence.Any(
                value => value.Contains(
                    "initiate payment",
                    StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void OriginalEvidenceRemainsPresentBesideDuplicateCandidate()
    {
        var snapshot = new MoneyService().BuildSnapshot(Now);

        Assert.Contains(
            snapshot.Evidence,
            x => x.ReviewState == EvidenceReviewState.Ready);

        Assert.Contains(
            snapshot.Evidence,
            x => x.ReviewState ==
                EvidenceReviewState.DuplicateCandidate);
    }
}
