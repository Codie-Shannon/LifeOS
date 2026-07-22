using LifeOS.Core.FinancialReview;

namespace LifeOS.Mobile.Views;

public sealed class FinancialReportsPage : ContentPage
{
    private static readonly DateTimeOffset ProofNow = new(2026, 7, 22, 12, 0, 0, TimeSpan.FromHours(12));
    public FinancialReportsPage()
    {
        Title = "Financial reports"; BackgroundColor = MoneyVisuals.Background;
        var proof = FinancialReviewProofData.Build(ProofNow);
        var report = new FinancialReportingService().Build(proof.FinancialRecords, proof.Candidates, new FinancialReportFilter(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)), ProofNow);
        var content = new VerticalStackLayout { Padding = new Thickness(18,14,18,30), Spacing = 13 };
        content.Children.Add(new Label { Text = "Financial reports", FontSize = 32, FontAttributes = FontAttributes.Bold, TextColor = Colors.White });
        content.Children.Add(new Label { Text = "Bounded period • currency-safe • compact review", FontSize = 14, TextColor = MoneyVisuals.Secondary });
        foreach (var x in report.CurrencyGroups) content.Children.Add(MoneyVisuals.CardView($"{x.Currency} summary", $"Income {x.Income:N2} • Expenses {x.Expenses:N2}\nOutstanding {x.OutstandingInvoices:N2} • Payments {x.PaymentsReceived:N2}", "NO MIXED TOTAL"));
        content.Children.Add(MoneyVisuals.CardView("Evidence completeness", $"Complete {report.Evidence.Count(x => x.State == EvidenceCompletenessState.Complete)} • Missing {report.Evidence.Count(x => x.State == EvidenceCompletenessState.Missing)}\nPending {report.Evidence.Count(x => x.State == EvidenceCompletenessState.PendingReview)} • Conflicting {report.Evidence.Count(x => x.State == EvidenceCompletenessState.Conflicting)}", "DRILL-DOWN ON DESKTOP"));
        content.Children.Add(MoneyVisuals.CardView("Review status", $"Unresolved candidates {report.UnresolvedCandidates.Count}\n{report.FreshnessNote}", "REVIEW-FIRST"));
        content.Children.Add(MoneyVisuals.CardView("Export boundary", "CSV/PDF export requires explicit preview, destination, confirmation and redaction controls. Exports never mutate authoritative records."));
        Content = new ScrollView { Content = content };
    }
}
