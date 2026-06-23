using LifeOS.Core.Agenda;
using LifeOS.Core.FollowUps;
using LifeOS.Core.Money;
using LifeOS.Core.PayLater;
using LifeOS.Core.WeeklyCloseOut;
namespace LifeOS.Shared.CommandCentre;

public sealed class CommandCentreSummary
{
    public required MoneyPressureSummary MoneyPressure { get; init; }

    public required FollowUpSummary FollowUps { get; init; }

    public required AgendaSummary Agenda { get; init; }

    public required PayLaterSummary PayLater { get; init; }

    public required WeeklyCloseOutSummary WeeklyCloseOut { get; init; }

    public required string OverallPressureLabel { get; init; }

    public required string NextSafestAction { get; init; }

    public required IReadOnlyList<string> Reasons { get; init; }
}
