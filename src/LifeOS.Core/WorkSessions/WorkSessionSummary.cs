namespace LifeOS.Core.WorkSessions;

public sealed class WorkSessionSummary
{
    public required int TotalSessions { get; init; }

    public required decimal TotalHours { get; init; }

    public required decimal BillableHours { get; init; }

    public required decimal BillableValue { get; init; }

    public required decimal PaidValue { get; init; }

    public required decimal UnpaidBillableValue { get; init; }

    public required int ActiveClientOrProjectCount { get; init; }

    public required IReadOnlyList<string> Reasons { get; init; }
}
