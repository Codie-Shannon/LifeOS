namespace LifeOS.Core.ProofTracker;

public sealed class ProofSummary
{
    public required int TotalProofItems { get; init; }

    public required int ReadyCount { get; init; }

    public required int SharedCount { get; init; }

    public required int AcceptedCount { get; init; }

    public required int ClientProofCount { get; init; }

    public required int RecentCount { get; init; }

    public required IReadOnlyList<string> Reasons { get; init; }
}
