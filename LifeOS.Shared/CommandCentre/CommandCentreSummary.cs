using LifeOS.Core.FollowUps;
using LifeOS.Core.Money;

namespace LifeOS.Shared.CommandCentre;

public sealed class CommandCentreSummary
{
    public required MoneyPressureSummary MoneyPressure { get; init; }

    public required FollowUpSummary FollowUps { get; init; }

    public required string OverallPressureLabel { get; init; }

    public required string NextSafestAction { get; init; }

    public required IReadOnlyList<string> Reasons { get; init; }
}