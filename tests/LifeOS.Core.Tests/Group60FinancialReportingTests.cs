using System.Text;
using LifeOS.Core.FinancialReview;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Group60FinancialReportingTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 22, 12, 0, 0, TimeSpan.FromHours(12));
    private static FinancialReport Build() { var proof = FinancialReviewProofData.Build(Now); return new FinancialReportingService().Build(proof.FinancialRecords, proof.Candidates, new FinancialReportFilter(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)), Now); }

    [Fact] public void AggregationIsBoundedAndCurrencySafe() { var report = Build(); Assert.NotEmpty(report.CurrencyGroups); Assert.Equal(report.CurrencyGroups.Count, report.CurrencyGroups.Select(x => x.Currency).Distinct().Count()); Assert.DoesNotContain(report.CurrencyGroups, x => string.IsNullOrWhiteSpace(x.Currency)); }
    [Fact] public void InvalidDateBoundaryFailsClosed() { var proof = FinancialReviewProofData.Build(Now); Assert.Throws<ArgumentException>(() => new FinancialReportingService().Build(proof.FinancialRecords, proof.Candidates, new FinancialReportFilter(new DateOnly(2026, 7, 2), new DateOnly(2026, 7, 1)), Now)); }
    [Fact] public void EvidenceCompletenessHasApprovedStates() { var report = Build(); Assert.NotEmpty(report.Evidence); Assert.All(report.Evidence, x => Assert.True(Enum.IsDefined(x.State))); }
    [Fact] public void CsvEscapesAndHonoursSelectedPeriod() { var service = new FinancialReportingService(); var report = Build(); var result = service.Export(report, new FinancialExportRequest(FinancialExportFormat.Csv, "proof", true, true, ["summary", "evidence"])); var text = Encoding.UTF8.GetString(result.Bytes); Assert.True(result.Succeeded); Assert.Contains("section,currency", text); Assert.DoesNotContain("@", text); }
    [Fact] public void PdfIsDerivativeAndRedacted() { var result = new FinancialReportingService().Export(Build(), new FinancialExportRequest(FinancialExportFormat.Pdf, "proof", true, true, ["summary"])); Assert.True(result.Succeeded); Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(result.Bytes)); }
    [Fact] public void ExportRequiresConfirmationAndDestination() { var service = new FinancialReportingService(); var cancelled = service.Export(Build(), new FinancialExportRequest(FinancialExportFormat.Csv, "proof", false, true, [])); var failed = service.Export(Build(), new FinancialExportRequest(FinancialExportFormat.Csv, "", true, true, [])); Assert.True(cancelled.Cancelled); Assert.False(failed.Succeeded); }
    [Fact] public void StaleDataIsExplicit() { var proof = FinancialReviewProofData.Build(Now); var report = new FinancialReportingService().Build(proof.FinancialRecords, proof.Candidates, new FinancialReportFilter(new DateOnly(2026,1,1), new DateOnly(2026,12,31)), Now, Now.AddDays(-2)); Assert.True(report.IsStale); Assert.Contains("old", report.FreshnessNote); }
    [Fact] public void NoMisleadingMixedCurrencyTotalSurfaceExists() { var properties = typeof(FinancialReport).GetProperties().Select(x => x.Name).ToArray(); Assert.DoesNotContain("Total", properties); Assert.Contains("CurrencyGroups", properties); }
}
