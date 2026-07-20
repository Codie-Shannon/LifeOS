using LifeOS.Core.FinancialRecords; using Xunit;
namespace LifeOS.Core.Tests;
public sealed class Group58FinancialRecordTests
{
 static readonly DateTimeOffset Now=new(2026,7,20,12,0,0,TimeSpan.FromHours(12));
 [Fact] public void ProofOverviewIsDeterministic(){var d=FinancialProofData.Build(Now);var x=FinancialRecordService.Summarize(d,"nzd");Assert.Equal("NZD",x.Currency);Assert.Equal(350m,x.Income);Assert.Equal(42.50m,x.Expenses);Assert.Equal(880m,x.InvoicesDue);}
 [Fact] public void PendingReviewExpenseIsExcluded(){var d=FinancialProofData.Build(Now);var x=FinancialRecordService.Summarize(d,"NZD");Assert.Equal(42.50m,x.Expenses);Assert.DoesNotContain(d.Transactions.Where(t=>t.ReviewState==FinancialReviewState.PendingReview).Select(t=>t.Amount),a=>a==x.Expenses);}
 [Theory][InlineData(0)][InlineData(12.345)] public void AmountNormalizationIsDeterministic(decimal input)=>Assert.Equal(decimal.Round(input,2,MidpointRounding.AwayFromZero),FinancialRecordService.NormalizeAmount(input));
 [Fact] public void NegativeAmountIsRejected()=>Assert.Throws<ArgumentOutOfRangeException>(()=>FinancialRecordService.NormalizeAmount(-1));
 [Fact] public void CurrencyValidationIsStrict(){Assert.Equal("NZD",FinancialRecordService.NormalizeCurrency("nzd"));Assert.Throws<ArgumentException>(()=>FinancialRecordService.NormalizeCurrency("NZ"));}
 [Fact] public void PartialAllocationIsCalculated(){var p=FinancialProofData.Build(Now).Payments[1];var x=FinancialRecordService.Allocate(p,100m);Assert.Equal(PaymentState.PartiallyAllocated,x.State);Assert.Equal(100m,x.Unallocated);}
 [Fact] public void AllocationCannotExceedPayment(){var p=FinancialProofData.Build(Now).Payments[1];Assert.Throws<InvalidOperationException>(()=>FinancialRecordService.Allocate(p,201m));}
 [Fact] public void DuplicateManualSubmissionIsDetected(){var d=FinancialProofData.Build(Now);Assert.True(FinancialRecordService.IsDuplicateSubmission(d.Transactions,"proof-income-1"));}
 [Fact] public void LinksCoverWorkProjectAndEvidence(){var d=FinancialProofData.Build(Now);Assert.Contains(d.Invoices.SelectMany(x=>x.Links),x=>x.Type==FinancialLinkType.Work);Assert.Contains(d.Invoices.SelectMany(x=>x.Links),x=>x.Type==FinancialLinkType.Project);Assert.NotEmpty(d.Invoices.SelectMany(x=>x.Evidence));}
 [Fact] public void AuditHistoryUsesSafeSummary(){var d=FinancialProofData.Build(Now);Assert.All(d.Audit,x=>Assert.DoesNotContain("@",x.SafeSummary));}
 [Fact] public void SensitiveValuesAreRedacted(){var x=FinancialRecordService.Redact("email test@example.com account 123456789");Assert.DoesNotContain("test@example.com",x);Assert.DoesNotContain("123456789",x);}
 [Fact] public void AccountsAreNeverPresentedAsLiveConnected(){Assert.All(FinancialProofData.Build(Now).Accounts,x=>Assert.False(x.IsLiveConnected));}
}
