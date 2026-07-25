using LifeOS.Core.PayLaterInsights;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups95To98PayLaterInsightTests
{
    private readonly PayLaterInsightService _service = new();

    [Fact]
    public void Afterpay_email_parses_totals_due_window_and_source()
    {
        PayLaterCandidate candidate = _service.Parse(
            "Afterpay statement",
            "Remaining $240.00. Next instalment $60.00 due 2026-08-01.",
            "fictional://mail/afterpay-1",
            Array.Empty<PayLaterCandidate>());

        Assert.Equal(PayLaterProvider.Afterpay, candidate.Provider);
        Assert.Equal(240m, candidate.Total);
        Assert.Equal(60m, candidate.NextDeduction);
        Assert.Equal(new DateOnly(2026, 8, 1), candidate.DueDate);
        Assert.Equal(PayLaterReviewState.Candidate, candidate.State);
    }

    [Fact]
    public void Duplicate_statement_is_not_silently_combined()
    {
        PayLaterCandidate first = Parse();
        PayLaterCandidate duplicate = _service.Parse(
            "Zip statement",
            "Total 120.00. Next deduction 30.00 due 2026-08-02.",
            "fictional://mail/zip-1",
            new[] { first });

        Assert.Equal(PayLaterReviewState.Duplicate, duplicate.State);
    }

    [Fact]
    public void Only_confirmed_deductions_are_excluded_from_safe_money()
    {
        PayLaterCandidate candidate = Parse();

        PayLaterDashboard before = _service.Summarize(new[] { candidate });
        PayLaterDashboard after = _service.Summarize(new[] { _service.Confirm(candidate) });

        Assert.Equal(0m, before.ExcludedFromSafeMoney);
        Assert.Equal(30m, after.ExcludedFromSafeMoney);
    }

    [Fact]
    public void Payment_and_autonomous_reconciliation_contracts_are_rejected()
    {
        ReadOnlyMoneyContract unsafeContract = new("Bank", true, true, true, true, true);

        Assert.Throws<InvalidOperationException>(() => _service.ValidateContract(unsafeContract));
    }

    private PayLaterCandidate Parse() =>
        _service.Parse(
            "Zip statement",
            "Total 120.00. Next deduction 30.00 due 2026-08-02.",
            "fictional://mail/zip-1",
            Array.Empty<PayLaterCandidate>());
}
