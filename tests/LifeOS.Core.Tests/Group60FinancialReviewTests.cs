using LifeOS.Core.Documents;
using LifeOS.Core.FinancialReview;
using LifeOS.Core.FinancialRecords;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Group60FinancialReviewTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 22, 12, 0, 0, TimeSpan.FromHours(12));

    [Fact]
    public void CandidateGenerationCoversEveryApprovedType()
    {
        var proof = FinancialReviewProofData.Build(Now);
        var types = proof.Candidates.Select(x => x.Type).ToHashSet();
        Assert.Contains(ReconciliationCandidateType.UnmatchedTransaction, types);
        Assert.Contains(ReconciliationCandidateType.InvoiceWithoutPayment, types);
        Assert.Contains(ReconciliationCandidateType.PaymentWithoutInvoice, types);
        Assert.Contains(ReconciliationCandidateType.PartialAllocation, types);
        Assert.Contains(ReconciliationCandidateType.AmountMismatch, types);
        Assert.Contains(ReconciliationCandidateType.DateMismatch, types);
        Assert.Contains(ReconciliationCandidateType.CurrencyMismatch, types);
        Assert.Contains(ReconciliationCandidateType.DuplicateCandidate, types);
        Assert.Contains(ReconciliationCandidateType.MissingEvidence, types);
    }

    [Fact]
    public void CandidateGenerationIsIdempotentAndPreservesReviewState()
    {
        var proof = FinancialReviewProofData.Build(Now);
        var service = new FinancialReviewService();
        var deferred = service.Resolve(proof.Candidates[0], new CandidateResolutionRequest(ReviewResolutionAction.Defer, "Review tomorrow", false), Now.AddMinutes(1)).Candidate;
        var existing = proof.Candidates.Select(x => x.Id == deferred.Id ? deferred : x).ToArray();
        var rerun = service.GenerateCandidates(proof.FinancialRecords, Now.AddMinutes(2), existing);
        Assert.Equal(rerun.Count, rerun.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(FinancialReviewStatus.Deferred, rerun.Single(x => x.Id == deferred.Id).Status);
    }

    [Fact]
    public void AllocationMathIsDeterministic()
    {
        var proof = FinancialReviewProofData.Build(Now);
        var invoice = proof.FinancialRecords.Invoices.Single(x => x.Id == "inv-58-c");
        var payment = proof.FinancialRecords.Payments.Single(x => x.Id == "pay-60-partial");
        var result = new FinancialReviewService().CalculateAllocation(invoice, payment);
        Assert.Equal(50m, result.ProposedAllocation);
        Assert.Equal(350m, result.RemainingInvoiceBalance);
        Assert.Equal(0m, result.RemainingPaymentBalance);
    }

    [Fact]
    public void TrustedStateActionsRequireExplicitConfirmation()
    {
        var service = new FinancialReviewService();
        var candidate = FinancialReviewProofData.Build(Now).Candidates.Single(x => x.AllocationIssue is not null);
        var rejected = service.Resolve(candidate, new CandidateResolutionRequest(ReviewResolutionAction.Allocate, "Allocate reviewed amount", false, AllocationAmount: 1m), Now);
        Assert.False(rejected.Applied);
        Assert.Equal(FinancialReviewStatus.Open, rejected.Candidate.Status);
    }

    [Fact]
    public void DeferDismissReopenAndExceptionFlowsAreAuditable()
    {
        var service = new FinancialReviewService();
        var candidate = FinancialReviewProofData.Build(Now).Candidates.First();
        var deferred = service.Resolve(candidate, new CandidateResolutionRequest(ReviewResolutionAction.Defer, "Later", false), Now).Candidate;
        var reopened = service.Resolve(deferred, new CandidateResolutionRequest(ReviewResolutionAction.Reopen, "Back to queue", false), Now.AddMinutes(1)).Candidate;
        var dismissed = service.Resolve(reopened, new CandidateResolutionRequest(ReviewResolutionAction.Dismiss, "Not applicable", false), Now.AddMinutes(2)).Candidate;
        var reopenedAgain = service.Resolve(dismissed, new CandidateResolutionRequest(ReviewResolutionAction.Reopen, "Reconsider", false), Now.AddMinutes(3)).Candidate;
        var exception = service.Resolve(reopenedAgain, new CandidateResolutionRequest(ReviewResolutionAction.AcceptAsException, "Known fictional exception", true), Now.AddMinutes(4)).Candidate;
        Assert.Equal(FinancialReviewStatus.ExceptionAccepted, exception.Status);
        Assert.Equal(5, exception.Resolutions.Count);
        Assert.All(exception.Audit, x => Assert.DoesNotContain("@", x.SafeSummary));
    }

    [Fact]
    public void AcceptedPreservedDocumentCanResolveEvidenceGapWithoutReplacement()
    {
        var proof = FinancialReviewProofData.Build(Now);
        var service = new FinancialReviewService();
        var candidate = proof.Candidates.First(x => x.EvidenceGap is not null);
        var document = proof.Documents.Single(x => x.State == DocumentIntakeState.Accepted);
        var originalHash = document.Original.Sha256;
        var result = service.LinkAcceptedEvidence(candidate, document, "Reviewed evidence link", true, Now);
        Assert.True(result.Applied);
        Assert.Equal(document.Id, result.Candidate.EvidenceGap!.LinkedDocumentId);
        Assert.Equal(originalHash, document.Original.Sha256);
        Assert.True(document.HasTrustedOriginal);
    }

    [Fact]
    public void ChangedAndRemovedSourcesFailClosed()
    {
        var service = new FinancialReviewService();
        var candidate = FinancialReviewProofData.Build(Now).Candidates.First(x => x.Sources.Count == 1);
        var changedSource = candidate.Sources[0] with { Fingerprint = "changed" };
        var changed = service.RefreshSourceState(candidate, [changedSource], Now);
        var removed = service.RefreshSourceState(candidate, [], Now);
        Assert.Equal(FinancialReviewStatus.SourceChanged, changed.Status);
        Assert.Equal(FinancialReviewStatus.SourceRemoved, removed.Status);
        Assert.False(service.Resolve(changed, new CandidateResolutionRequest(ReviewResolutionAction.Dismiss, "No silent recovery", false), Now).Applied);
    }

    [Fact]
    public void FiltersAndSharedSummaryRemainDeterministic()
    {
        var proof = FinancialReviewProofData.Build(Now);
        var service = new FinancialReviewService();
        var critical = service.Filter(proof.Candidates, new FinancialReviewFilter(Severity: FinancialReviewSeverity.Critical));
        var summary = service.Summarize(proof.Candidates);
        Assert.NotEmpty(critical);
        Assert.Equal(proof.Candidates.Count, summary.Open);
        Assert.Equal(critical.Count, summary.Critical);
        Assert.True(summary.EvidenceGaps > 0);
        Assert.True(summary.AllocationIssues > 0);
    }

    [Fact]
    public void DiagnosticsAreRedacted()
    {
        var diagnostic = new FinancialReviewService().Redact("person@example.com account 123456789");
        Assert.DoesNotContain("person@example.com", diagnostic);
        Assert.DoesNotContain("123456789", diagnostic);
    }

    [Fact]
    public void ForbiddenExternalWriteCapabilitiesRemainAbsent()
    {
        var serviceType = typeof(FinancialReviewService);
        var methodNames = serviceType.GetMethods().Select(x => x.Name).ToArray();
        Assert.DoesNotContain(methodNames, x => x.Contains("Bank", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(methodNames, x => x.Contains("ProviderWrite", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(methodNames, x => x.Contains("InitiatePayment", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(methodNames, x => x.Contains("AutoReconcile", StringComparison.OrdinalIgnoreCase));
    }
}
