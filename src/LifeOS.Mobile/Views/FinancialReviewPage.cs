using LifeOS.Core.FinancialReview;

namespace LifeOS.Mobile.Views;

public sealed class FinancialReviewPage : ContentPage
{
    private static readonly DateTimeOffset ProofNow = new(2026, 7, 22, 12, 0, 0, TimeSpan.FromHours(12));
    private readonly FinancialReviewService _service = new();
    private readonly VerticalStackLayout _content = new();
    private IReadOnlyList<ReconciliationCandidate> _candidates = FinancialReviewProofData.Build(ProofNow).Candidates;

    public FinancialReviewPage()
    {
        Title = "Financial review";
        BackgroundColor = MoneyVisuals.Background;
        Content = new ScrollView { Content = _content };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Render();
    }

    private void Render()
    {
        _content.Children.Clear();
        _content.Padding = new Thickness(18, 14, 18, 30);
        _content.Spacing = 13;
        var summary = _service.Summarize(_candidates);
        _content.Children.Add(new Label { Text = "Financial review", FontSize = 32, FontAttributes = FontAttributes.Bold, TextColor = Colors.White });
        _content.Children.Add(new Label { Text = "Shared review contracts • explicit confirm or defer • no provider writes", FontSize = 14, TextColor = MoneyVisuals.Secondary });
        _content.Children.Add(MoneyVisuals.CardView("Review status", $"Open {summary.Open} • Critical {summary.Critical}\nEvidence gaps {summary.EvidenceGaps} • Allocation issues {summary.AllocationIssues}", "REVIEW-FIRST"));

        foreach (var candidate in _candidates.Where(x => x.Status == FinancialReviewStatus.Open).Take(5))
        {
            var captured = candidate;
            _content.Children.Add(MoneyVisuals.CardView(candidate.Type.ToString(), $"{candidate.Explanation}\n{candidate.Currency} • {candidate.RecordDate:dd MMM} • {candidate.Severity}", candidate.Status.ToString()));
            var defer = MoneyVisuals.Action("Defer", Color.FromArgb("#2C3140"));
            defer.Clicked += (_, _) => Apply(captured, new CandidateResolutionRequest(ReviewResolutionAction.Defer, "Deferred from Full Mobile review", false));
            _content.Children.Add(defer);

            if (candidate.AllocationIssue is { ProposedAllocation: > 0m } allocation)
            {
                var confirm = MoneyVisuals.Action($"Confirm local allocation {candidate.Currency} {allocation.ProposedAllocation:N2}", Color.FromArgb("#4056C7"));
                confirm.Clicked += (_, _) => Apply(captured, new CandidateResolutionRequest(ReviewResolutionAction.Allocate, "Confirmed from Full Mobile review", true, AllocationAmount: allocation.ProposedAllocation));
                _content.Children.Add(confirm);
            }
            else if (candidate.EvidenceGap is null)
            {
                var confirm = MoneyVisuals.Action("Accept as reviewed exception", Color.FromArgb("#4056C7"));
                confirm.Clicked += (_, _) => Apply(captured, new CandidateResolutionRequest(ReviewResolutionAction.AcceptAsException, "Confirmed fictional exception from Full Mobile", true));
                _content.Children.Add(confirm);
            }
        }
        _content.Children.Add(MoneyVisuals.CardView("Boundary", "Mobile actions update only the local review candidate. No automatic reconciliation, payment initiation, accounting approval or external write exists."));
    }

    private void Apply(ReconciliationCandidate candidate, CandidateResolutionRequest request)
    {
        var result = _service.Resolve(candidate, request, ProofNow.AddMinutes(20));
        if (result.Applied)
            _candidates = _candidates.Select(x => x.Id == result.Candidate.Id ? result.Candidate : x).ToArray();
        Render();
    }
}
