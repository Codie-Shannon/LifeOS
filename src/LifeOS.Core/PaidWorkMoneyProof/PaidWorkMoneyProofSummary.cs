namespace LifeOS.Core.PaidWorkMoneyProof;

public sealed class PaidWorkMoneyProofSummary
{
    public required PaidWorkMoneyProofHealth Health { get; init; }

    public required decimal TotalBillableValue { get; init; }

    public required decimal PaidValue { get; init; }

    public required decimal UnpaidBillableValue { get; init; }

    public required decimal InvoiceReadyValue { get; init; }

    public required decimal PendingPipelineValue { get; init; }

    public required decimal PendingManualIncome { get; init; }

    public required decimal MoneyAtRisk { get; init; }

    public required int InvoiceReadySessionCount { get; init; }

    public required int ReadyProofCount { get; init; }

    public required int ClientProofCount { get; init; }

    public required int TimesheetsNeeded { get; init; }

    public required int InvoicesNeeded { get; init; }

    public required int PaymentsExpected { get; init; }

    public required int AdminActionCount { get; init; }

    public required IReadOnlyList<PaidWorkMoneyProofFocusItem> FocusItems { get; init; }

    public required IReadOnlyList<string> Reasons { get; init; }

    public required string CopyReadyClientUpdate { get; init; }
}
