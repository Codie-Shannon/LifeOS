namespace LifeOS.Core.CommandCentre;

public static class CommandCentreSignalSorter
{
    public static IReadOnlyList<CommandCentreSignal> SortForToday(IEnumerable<CommandCentreSignal> signals)
    {
        ArgumentNullException.ThrowIfNull(signals);

        return signals
            .Where(signal => signal.IsTodayVisible)
            .OrderBy(signal => signal.SortWeight)
            .ThenByDescending(signal => signal.Priority)
            .ThenBy(signal => signal.DueDate ?? DateOnly.MaxValue)
            .ThenBy(signal => signal.Title)
            .ToList();
    }
}
